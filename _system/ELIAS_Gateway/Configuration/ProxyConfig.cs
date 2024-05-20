using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;
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
      private readonly static string query   = "query";

      private readonly static string error_1 = "Invalid ELIAS_SERVICES environment variable. Expecting: [service_name - alphanumeric and _]:[port], ..., [service_name - alphanumeric and _]:[port].";
      private readonly static string error_2 = "Invalid ELIAS_DOMAIN environment variable. Expecting: domain name, e.g. domain or domain.tld.";
      private readonly static string error_3 = "Invalid TLS_CERT_EMAIL environment variable. A valid email address is expected, or 'none' when HTTPS protocol cannot be used.";
      private readonly static string error_4 = "Invalid DOW3_TO_SERVICE environment variable. The service name {0} requested to map to the domain and WWW. sub-domain is unknown or cannot be mapped to them.";
      private readonly static string error_5 = "Invalid SERVICE_ALIAS environment variable. The SERVICE_ALIAS parameter can take values ON, OFF or be omitted defaulting to OFF. When SERVICE_ALIAS = ON, services can be accessed as [service].domain.tld in addition to the standard [service].elias.domain.tld.";

      public static IReadOnlyList< RouteConfig > Get_routes()
      {
         // route_[service]: { "ClusterId": "cluster_[service]", "Match": { "Hosts": [ "[service].elias.domain" ], "Path": "{**catch-all}" } }
         var routes = new List< RouteConfig >();


         // Get the domain.
         string? domain = Environment.GetEnvironmentVariable( "ELIAS_DOMAIN" );

         if( string.IsNullOrWhiteSpace( domain ) )
         {
            throw new Exception( error_2 );
         }


         // Get the additional service mapping request.
         bool service_dot_domain_alias = ( Environment.GetEnvironmentVariable( "SERVICE_ALIAS" ) ?? "" ).ToUpper() switch
         {
            "ON" => true,
            "OFF" => false,
            "" => false,
            _ => throw new Exception( error_5 ),
         };


         var services = Get_services();
         Get_the_domain_mapped_service( services, out string dow3_mapped_service );


         foreach( var service in services )
         {
            var addresses = new List< string >( [ $"{service.Name}.elias.{domain}" ] );

            if( service_dot_domain_alias)
            {
               addresses.Add( 0 == string.Compare( service.Name, query, true ) ? $"elias.{domain}" : $"{service.Name}.{domain}" );
            }

            if( 0 == string.Compare( service.Name, dow3_mapped_service, true ) )
            {
               addresses.Add( domain );
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
            clusters.Add(  new ClusterConfig
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
         string? domain = Environment.GetEnvironmentVariable( "ELIAS_DOMAIN" );

         if( string.IsNullOrWhiteSpace( domain ) )
         {
            throw new Exception( error_2 );
         }


         var domains = new List< string >();
         var services = Get_services();
         
         
         // See if the domain is mapped to a service.
         if( Get_the_domain_mapped_service( services, out _ ) )
         {
            domains.Add( domain );
            domains.Add( $"www.{domain}" );
         }


         foreach( var service in services )
         {
            domains.Add( $"{service.Name}.elias.{domain}" );
            domains.Add( 0 == string.Compare( service.Name, query, true ) ? $"elias.{domain}" : $"{service.Name}.{domain}" );
         }

         return domains;
      }

      private static bool Get_the_domain_mapped_service( HashSet< ELIAS_Service > services, out string dow3_mapped_service )
      {
         dow3_mapped_service = Environment.GetEnvironmentVariable( "DOW3_TO_SERVICE" ) ?? "";

         if( string.IsNullOrWhiteSpace( dow3_mapped_service ) )
         {
            return false;
         }

         foreach( var service in services )
         {
            if( 0 == string.Compare( service.Name, dow3_mapped_service, true ) )
            {
               // A service is mapped to the domain.
               if( 0 == string.Compare( dow3_mapped_service, query, true ) )
               {
                  // The query service cannot be mapped to the domain.
                  throw new Exception( string.Format( error_4, dow3_mapped_service ) );
               }

               return true;
            }
         }

         // Could not find the mapped service - a spelling error perhaps???
         throw new Exception( string.Format( error_4, dow3_mapped_service ) );
      }

      private static HashSet< ELIAS_Service > Get_services()
      {
         string? service_raw = Environment.GetEnvironmentVariable( "ELIAS_SERVICES" );


         if( string.IsNullOrWhiteSpace( service_raw ) )
         {
            throw new Exception( error_1 );
         }


         var services = new List< ELIAS_Service >();
         bool has_query = false;

         foreach( var service_data in service_raw.Split( ',', StringSplitOptions.TrimEntries ) )
         {
            string[] service = service_data.Split( ':', StringSplitOptions.TrimEntries );


            if( 2 != service.Length )
            {
               throw new Exception( error_1 );
            }


            if( string.IsNullOrWhiteSpace( service[0] ) )
            {
               throw new Exception( error_1 );
            }


            if( !service[0].All( c => char.IsAsciiLetterOrDigit( c ) || c == '_' ) )
            {
               throw new Exception( error_1 );
            }


            if( !ushort.TryParse( service[1], out var port ) )
            {
               throw new Exception( error_1 );
            }


            has_query |= 0 == string.Compare( service[0], query, true );


            services.Add( new ELIAS_Service( service[0], port ));
         }


         if( !has_query )
         {
            services.Add( new ELIAS_Service( query, 80 ));
         }


         if( 1 > services.Count )
         {
            throw new Exception( error_1 );
         }


         var services_ready = services.ToHashSet();

         if( services_ready.Count != services.Count )
         {
            throw new Exception( error_1 );
         }


         return services_ready;
      }

      /// <summary>
      ///  Gets the email address associated with TLS certificates acquired from Let's Encrypt.
      ///  Environment parameter: TLS_CERT_EMAIL
      ///  Status: Required
      ///  Values: [a valid email address] or "none"
      ///  Set TLS_CERT_EMAIL: "[a valid email]" - to requite HTTPS protocol when accessing the domain gateway from the outside world.
      ///  Set TLS_CERT_EMAIL: "none"            - to enable use of HTTP protocol when accessing the domain gateway from the outside world.
      /// </summary>
      /// <returns>The provided email address, or an empty string.</returns>
      /// <exception cref="Exception">Exception( [description] )</exception>
      /// <remarks>This parameter does not relate to the use of HTTP/HTTPS protocol between the domain gateway and the services on the internal ELIAS services network.</remarks>
      public static string Get_TLS_CERT_EMAIL()
      {
         string email = Environment.GetEnvironmentVariable( "TLS_CERT_EMAIL" ) ?? "";

         if( string.IsNullOrWhiteSpace( email ) )
         {
            throw new Exception( error_3 );
         }


         if( 0 == string.Compare( email, "none", true ) )
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

         throw new Exception( error_3 );
      }


      public static bool Force_HTTPS_on_ELIAS_gateway()
      {
         if( string.IsNullOrWhiteSpace( Get_TLS_CERT_EMAIL() ) )
         {
            return false;
         }

         return Launch_settings.Force_HTTPS_on_ELIAS_service( Launch_settings.Protocol_permission.Allowed, Launch_settings.Protocol_permission.Allowed, false );
      }
   }
}
