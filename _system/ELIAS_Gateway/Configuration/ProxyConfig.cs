﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;
using Org.BouncyCastle.Crypto.Prng;
using Settings;
using Yarp.ReverseProxy.Configuration;

namespace ELIAS_Gateway.Configuration
{
   public sealed class ELIAS_Service( string _name, ushort _port ) : IEqualityComparer< ELIAS_Service >
   {
      public string Name { get; init; } = _name;
      public string Host { get; init; } = $"elias_{_name}_service";
      public ushort Port { get; init; } = _port;

      public bool Equals( ELIAS_Service? x, ELIAS_Service? y )
      {
         if( null == x || null == y )
         {
            return false;
         }

         return x.Name.Equals( y.Name );
      }

      public int GetHashCode( [DisallowNull] ELIAS_Service obj )
      {
         return obj.Name.GetHashCode();
      }
   }

   public class Proxy_Config
   {
      private readonly static string query = "query";
      private readonly static ushort query_service_default_port = 8080;

      private readonly static string ELIAS_DOMAIN = "ELIAS_DOMAIN";
      private readonly static string SERVICE_2_ED = "SERVICE_2_ED";
      private readonly static string SERVICE_2_W3 = "SERVICE_2_W3";
      private readonly static string SERVICE_2_SD = "SERVICE_2_SD";
      private readonly static string TLSCERT_MAIL = "TLSCERT_MAIL";
      private readonly static string ELIAS_SERVICES = "ELIAS_SERVICES";
      private readonly static string GATEWAY_IN_HTTPS_PORT = "ASPNETCORE_HTTPS_PORTS";

      private readonly static string error_1 = "Invalid {0} environment variable. Expecting: [service_name - alphanumeric and _]:[port], ..., [service_name - alphanumeric and _]:[port].";
      private readonly static string error_2 = "Invalid {0} environment variable. Expecting: domain name, e.g. domain or domain.tld.";
      private readonly static string error_3 = "Invalid {0} environment variable. The service name {1} requested to map is unknown or cannot be mapped.";
      private readonly static string error_4 = "Invalid {0} environment variable. The parameter can take values ON or OFF. When ON, services can be accessed as [service].domain.tld in addition to the standard [service].elias.domain.tld.";
      private readonly static string error_5 = "Invalid {0} environment variable. A valid email address is expected, or '-' when Let's Encrypt certificate is not required.";

      public static IReadOnlyList< RouteConfig > Get_routes()
      {
         // route_[service]: { "ClusterId": "cluster_[service]", "Match": { "Hosts": [ "[service].elias.domain" ], "Path": "{**catch-all}" } }
         var routes = new List< RouteConfig >();


         // Get the domain.
         string? domain = Environment.GetEnvironmentVariable( ELIAS_DOMAIN );

         if( string.IsNullOrWhiteSpace( domain ) )
         {
            throw new Exception( string.Format( error_2, ELIAS_DOMAIN ) );
         }


         // Get the additional service mapping request.
         bool service_to_sub_domain = Get_service_to_sub_domain_status();

         var services = Get_services();
         Get_the_domain_mapped_service( services, SERVICE_2_ED, out string ed_mapped_service );
         Get_the_domain_mapped_service( services, SERVICE_2_W3, out string w3_mapped_service );


         foreach( var service in services )
         {
            var addresses = new List< string >( [ $"{service.Name}.elias.{domain}" ] );

            if( service_to_sub_domain)
            {
               Add_direct_sub_domain_access( service, domain, ref addresses );
            }

            if( 0 == string.Compare( service.Name, ed_mapped_service, true ) )
            {
               addresses.Add( domain );
            }

            if( 0 == string.Compare( service.Name, w3_mapped_service, true ) )
            {
               addresses.Add( $"www.{domain}" );
            }

            routes.Add( new RouteConfig
            {
               RouteId   = $"route_{service.Name}",
               ClusterId = $"cluster_{service.Name}",
               Match = new RouteMatch
               {
                  Hosts = addresses,
                  Path  = "{**catch-all}"
               }
            } );
         }

         return routes;
      }

      public static IReadOnlyList< ClusterConfig > Get_clusters()
      {
         // cluster_[service]: { "Destinations": { "destination1": { "Address": "http://elias_query_service:[port]" } }
         var clusters = new List< ClusterConfig >();

         foreach( var service in Get_services() )
         {
            clusters.Add( new ClusterConfig
            {
               ClusterId = $"cluster_{service.Name}",
               Destinations = new Dictionary< string, DestinationConfig >
               {
                  { "destination1", new DestinationConfig { Address = $"http://{service.Host}:{service.Port}" } }
               }
            } );
         }

         return clusters;
      }

      public static IReadOnlyList< string > Get_domains_for_TLS()
      {
         // Get the domain.
         string? domain = Environment.GetEnvironmentVariable( ELIAS_DOMAIN );

         if( string.IsNullOrWhiteSpace( domain ) )
         {
            throw new Exception( string.Format( error_2, ELIAS_DOMAIN ) );
         }


         var domains = new List< string >();
         var services = Get_services();
         
         
         // See if the domain is mapped to a service.
         if( Get_the_domain_mapped_service( services, SERVICE_2_ED, out _ ) )
         {
            domains.Add( domain );
         }

         if( Get_the_domain_mapped_service( services, SERVICE_2_W3, out _ ) )
         {
            domains.Add( $"www.{domain}" );
         }

         bool service_to_sub_domain = Get_service_to_sub_domain_status();

         foreach( var service in services )
         {
            domains.Add( $"{service.Name}.elias.{domain}" );

            if( service_to_sub_domain)
            {
               Add_direct_sub_domain_access( service, domain, ref domains );
            }
         }

         return domains;
      }

      public static void Add_direct_sub_domain_access( ELIAS_Service service, string domain, ref List< string > addresses )
      {
         if( 0 == string.Compare( service.Name, query, true ) )
         {
            addresses.Add( $"elias.{domain}" );
         }
         else
         {
            addresses.Add( $"{service.Name}.{domain}" );
         }
      }

      private static bool Get_the_domain_mapped_service( HashSet< ELIAS_Service > services, string service_mapping_name, out string mapped_service )
      {
         mapped_service = Environment.GetEnvironmentVariable( service_mapping_name ) ?? "";

         if( string.IsNullOrWhiteSpace( mapped_service ) )
         {
            return false;
         }

         if( 0 == string.Compare( mapped_service, "-", false ) )
         {
            mapped_service = "";

            return false;
         }

         foreach( var service in services )
         {
            if( 0 == string.Compare( service.Name, mapped_service, true ) )
            {
               // A service is mapped to the domain.
               if( 0 == string.Compare( mapped_service, query, true ) )
               {
                  // The query service cannot be mapped to the domain.
                  throw new Exception( string.Format( error_3, service_mapping_name, mapped_service ) );
               }

               return true;
            }
         }

         // Could not find the mapped service - a spelling error perhaps???
         throw new Exception( string.Format( error_3, service_mapping_name, mapped_service ) );
      }

      private static bool Get_service_to_sub_domain_status()
      {
         string service_to_sub_domain_status = Environment.GetEnvironmentVariable( SERVICE_2_SD ) ?? "";

         switch( service_to_sub_domain_status.ToUpper() )
         {
         case "ON":  return true;
         case "OFF": return false;
         };

         throw new Exception( string.Format( error_4, SERVICE_2_SD ) );
      }

      private static HashSet< ELIAS_Service > Get_services()
      {
         string? service_raw = Environment.GetEnvironmentVariable( ELIAS_SERVICES );


         if( string.IsNullOrWhiteSpace( service_raw ) )
         {
            throw new Exception( string.Format( error_1, ELIAS_SERVICES ) );
         }


         var services = new List< ELIAS_Service >();
         bool has_query = false;

         foreach( var service_data in service_raw.Split( ',', StringSplitOptions.TrimEntries ) )
         {
            string[] service = service_data.Split( ':', StringSplitOptions.TrimEntries );


            if( 2 != service.Length )
            {
               throw new Exception( string.Format( error_1, ELIAS_SERVICES ) );
            }


            if( string.IsNullOrWhiteSpace( service[0] ) )
            {
               throw new Exception( string.Format( error_1, ELIAS_SERVICES ) );
            }


            if( !service[0].All( c => char.IsAsciiLetterOrDigit( c ) || c == '_' ) )
            {
               throw new Exception( string.Format( error_1, ELIAS_SERVICES ) );
            }


            if( !ushort.TryParse( service[1], out var port ) )
            {
               throw new Exception( string.Format( error_1, ELIAS_SERVICES ) );
            }


            has_query |= 0 == string.Compare( service[0], query, true );


            services.Add( new ELIAS_Service( service[0], port ));
         }


         if( !has_query )
         {
            services.Add( new ELIAS_Service( query, query_service_default_port ));
         }


         if( 1 > services.Count )
         {
            throw new Exception( string.Format( error_1, ELIAS_SERVICES ) );
         }


         var services_ready = services.ToHashSet();

         if( services_ready.Count != services.Count )
         {
            throw new Exception( string.Format( error_1, ELIAS_SERVICES ) );
         }


         return services_ready;
      }

      /// <summary>
      ///  Gets the email address associated with TLS certificates acquired from Let's Encrypt.
      ///  Environment parameter: TLSCERT_MAIL
      ///  Status: Required
      ///  Values: [a valid email address] or "none"
      ///  Set TLSCERT_MAIL: "[a valid email]" - to requite HTTPS protocol when accessing the domain gateway from the outside world.
      ///  Set TLSCERT_MAIL: "none"            - to enable use of HTTP protocol when accessing the domain gateway from the outside world.
      /// </summary>
      /// <returns>The provided email address, or an empty string.</returns>
      /// <exception cref="Exception">Exception( [description] )</exception>
      /// <remarks>This parameter does not relate to the use of HTTP/HTTPS protocol between the domain gateway and the services on the internal ELIAS services network.</remarks>
      public static string Get_TLS_CERT_EMAIL()
      {
         if( !ushort.TryParse( Environment.GetEnvironmentVariable( GATEWAY_IN_HTTPS_PORT ), out _ ) )
         {
            return "";
         }
         
         string email = Environment.GetEnvironmentVariable( TLSCERT_MAIL ) ?? "";

         if( string.IsNullOrWhiteSpace( email ) )
         {
            throw new Exception( string.Format( error_5, TLSCERT_MAIL ) );
         }

         if( 0 == string.Compare( email, "-", true ) )
         {
            return "";
         }

         try
         {
            // Normalize the domain
            string DomainMapper( Match match )
            {
               // Examine the domain part of the email and normalize it.
               // Use IdnMapping class to convert Unicode domain names.
               var idn = new IdnMapping();

               // Pull out and process domain name (throws ArgumentException on invalid)
               string domainName = idn.GetAscii( match.Groups[2].Value );

               return match.Groups[1].Value + domainName;
            }

            email = Regex.Replace( email, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds( 200 ) );

            if( Regex.IsMatch( email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds( 250 ) ) )
            {
               return email;
            }
         }
         catch
         {
         }

         throw new Exception( string.Format( error_5, TLSCERT_MAIL ) );
      }


      public static bool Force_HTTPS_on_ELIAS_gateway()
      {
         return Launch_settings.Force_HTTPS_on_ELIAS_service( Launch_settings.Protocol_permission.Allowed, Launch_settings.Protocol_permission.Allowed, false );
      }
   }
}
