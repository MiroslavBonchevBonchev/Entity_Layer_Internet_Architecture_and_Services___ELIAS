using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settings
{
   public class Launch_settings
   {
      private static readonly string ASPNETCORE_HTTP_PORTS  = "ASPNETCORE_HTTP_PORTS";
      private static readonly string ASPNETCORE_HTTPS_PORTS = "ASPNETCORE_HTTPS_PORTS";
      private static readonly string HTTPS_REDIRECT         = "HTTPS_REDIRECT";

      public enum Protocol_permission
      {
         Banned,
         Allowed,
         Required
      }


      /// <summary>
      /// Test if HTTP/HTTPS protocol request is OK. For internal use.
      /// </summary>
      /// <param name="protocol_status">The required status as per the needs of the service.</param>
      /// <param name="is_http_name ">HTTP or HTTPS environment parameter name.</param>
      /// <returns>true - if port is provided, false - if port is not provided, exception if error.</returns>
      /// <exception cref="Exception"></exception>
      private static bool Is_protocol_OK( Protocol_permission protocol_status, bool is_http_name, out ushort port )
      {
         port                   = 0;
         string  parameter_name = is_http_name ? ASPNETCORE_HTTP_PORTS : ASPNETCORE_HTTPS_PORTS;
         string? parameter_value = Environment.GetEnvironmentVariable( parameter_name );

         switch( protocol_status )
         {
         case Protocol_permission.Banned:
            if( !string.IsNullOrWhiteSpace( parameter_value ) )
            {
               throw new Exception( $"Invalid environment variable: {parameter_name} - the variable is not permitted for this service by its author(s)." );
            }
            return false;

         case Protocol_permission.Required:
            if( string.IsNullOrWhiteSpace( parameter_value ) )
            {
               throw new Exception( $"Invalid environment variable: {parameter_name} - the variable is required for this service but is not defined." );
            }
            break;
         }

         if( string.IsNullOrWhiteSpace( parameter_value ) )
         {
            return false;
         }

         if( !ushort.TryParse( parameter_value, out port ) )
         {
            throw new Exception( $"Invalid environment variable: {parameter_name} - must be a valid port." );
         }

         return true;
      }

      /// <summary>
      /// Test if the HTTP/HTTPS protocol request is OK, and whether HTTPS should be enforced. Applicable environment variables are:
      ///   - ASPNETCORE_HTTP_PORTS
      ///   - ASPNETCORE_HTTPS_PORTS
      ///   - HTTPS_REDIRECT
      /// </summary>
      /// <param name="http_status">Is HTTP Banned, Allowed or Required. "Required" means that the protocol must be available, not necessarily enforced.</param>
      /// <param name="https_status">Is HTTPS Banned, Allowed or Required. "Required" means that the protocol must be available, not necessarily enforced.</param>
      /// <param name="are_exclusive">Only used when the two protocols are both enabled.</param>
      /// <returns></returns>
      /// <exception cref="Exception"></exception>
      public static bool Force_HTTPS_on_ELIAS_service( Protocol_permission http_status, Protocol_permission https_status, bool are_exclusive )
      {
         bool has_http__port = Is_protocol_OK( http_status, true, out ushort http__port );
         bool has_https_port = Is_protocol_OK( https_status, false, out ushort https_port );

         if( has_http__port && has_https_port )
         {
            if( are_exclusive )
            {
               throw new Exception( $"Bad call to Force_HTTPS_on_ELIAS_service( ... ) - inconsistent call conditions." );
            } 

            if( http__port == https_port )
            {
               throw new Exception( $"Invalid environment protocol variables: {ASPNETCORE_HTTP_PORTS} and {ASPNETCORE_HTTPS_PORTS} require the same port." );
            }
         }

         if( !has_http__port && !has_https_port )
         {
            throw new Exception( $"Invalid environment protocol variable(s): {ASPNETCORE_HTTP_PORTS} is {http_status}; {ASPNETCORE_HTTPS_PORTS} is {https_status}." );
         }


         switch( (Environment.GetEnvironmentVariable( HTTPS_REDIRECT ) ?? "").ToUpper() )
         {
         case "ON":
            if( Protocol_permission.Banned == https_status )
            {
               throw new Exception( $"Bad call to Force_HTTPS_on_ELIAS_service( ... ) - inconsistent call conditions." );
            }

            if( has_https_port )
            {
               return true;
            }
            break;
 
         case "OFF":
            break;

         case "":
            if( has_https_port )
            {
               throw new Exception( $"Invalid environment variable: {HTTPS_REDIRECT} - the variable must be set to 'ON' or 'OFF'." );
            }
            break;

         default:
            throw new Exception( $"Invalid environment variable: {HTTPS_REDIRECT} - the variable must be set to 'ON' or 'OFF'." );
         }


         switch( https_status )
         {
         case Protocol_permission.Required:
            switch(  http_status )
            {
            case Protocol_permission.Required: return false;
            case Protocol_permission.Allowed:  return !has_http__port;
            case Protocol_permission.Banned:   return true;
            }
            break;

         case Protocol_permission.Allowed:
            switch(  http_status )
            {
            case Protocol_permission.Required: return false;
            case Protocol_permission.Allowed:  return has_https_port && !has_http__port;
            case Protocol_permission.Banned:   if( has_https_port ) { return true; } throw new Exception( $"Invalid environment protocol variable: {ASPNETCORE_HTTPS_PORTS} is required but not provided." );
            }
            break;

         case Protocol_permission.Banned:
            switch(  http_status )
            {
            case Protocol_permission.Required: return false;
            case Protocol_permission.Allowed:  if( has_http__port ) { return false; } throw new Exception( $"Invalid environment protocol variable: {ASPNETCORE_HTTP_PORTS} is required but not provided." );
            case Protocol_permission.Banned:   throw new Exception( $"Bad call to Force_HTTPS_on_ELIAS_service( ... ) - inconsistent call conditions." );
            }
            break;
         }

         return false;
      }
   }
}
