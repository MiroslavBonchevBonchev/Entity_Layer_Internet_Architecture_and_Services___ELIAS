using ELIAS_Gateway.Configuration;
using LettuceEncrypt;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Settings;
using Yarp.ReverseProxy.Transforms;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Rewrite;



var builder = WebApplication.CreateBuilder(args);



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddReverseProxy()
   .AddTransforms( context =>
   {
      context.CopyRequestHeaders = true;
      context.AddOriginalHost( useOriginal: true );
      context.UseDefaultForwarders = true;

      context.AddXForwardedFor();
      context.AddXForwardedHost();
      context.AddXForwardedPrefix();
      context.AddXForwardedProto();
   } )
   .LoadFromMemory( Proxy_Config.Get_routes( null ), Proxy_Config.Get_clusters() );



// Add LettuceEncrypt when the HTTPS protocol is required, MBB
string lets_encrypt_certificate_email = Proxy_Config.Get_TLS_CERT_EMAIL();

if( !string.IsNullOrWhiteSpace( lets_encrypt_certificate_email ) )
{
   builder.Services.AddLettuceEncrypt( context => {
      context.AcceptTermsOfService = true;
      context.EmailAddress = lets_encrypt_certificate_email;
      context.DomainNames  = [.. Proxy_Config.Get_domains_for_TLS( null )]; }

      // 1. In order for the PersistDataToDirectory( ... ) to work, the Dockerfile requires creating of a /home/app/https_certificates directory:
      //    FROM base AS final                      # - already there
      //    WORKDIR /home/app/https_certificates    # - required line, can be in /app/https_certificates as well. Note that direct deployment to linux does not need any additional work (like this line).
      //    WORKDIR /app                            # - already there
      // 2. To persist the certificates and other app data, add to the docker-compose.yml
      //    volumes:
      //       - 
   ).PersistDataToDirectory( new DirectoryInfo( "/home/app/https_certificates" ), "Password123" );
}



// Prevent WARNING: Storing keys in a directory '/home/app/.aspnet/DataProtection-Keys' that may not be persisted outside of the container. MBB
builder.Services.AddDataProtection()
   .PersistKeysToFileSystem( new DirectoryInfo( $"/home/app/data_protection_keys" ) )
   .UseCryptographicAlgorithms( new AuthenticatedEncryptorConfiguration()
   {
      EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
      ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
   } );



var app = builder.Build();

if( app.Environment.IsDevelopment() )
{
   foreach( var route in Proxy_Config.Get_routes( null ) )
   {
      Console.WriteLine( $"RouteId: {route.RouteId} - ClusterId : {route.ClusterId} - Addresses: {string.Join( ", ", null != route.Match && null != route.Match.Hosts ? route.Match.Hosts : new List< string >() )}"  );
   }
   
   foreach( var cluster in Proxy_Config.Get_clusters() )
   {
      Console.WriteLine( $"ClusterId : {cluster.ClusterId} - Addresses: {(null != cluster.Destinations && 0 != cluster.Destinations.Count ? cluster.Destinations[cluster.Destinations.Keys.First()].Address : "") }" );
   }

   foreach( var domain in Proxy_Config.Get_domains_for_TLS( null ) )
   {
      Console.WriteLine( $"TLS Domain : {domain}"  );
   }

   app.UseSwagger();
   app.UseSwaggerUI();
}



// Require HTTPS conditionally, MBB.
if( Proxy_Config.Force_HTTPS_on_ELIAS_gateway() )
{
   app.UseHttpsRedirection();
}



// Set the domain to w3 redirection type, if any, MBB.
switch( Proxy_Config.Get_domain_w3_redirection_type() )
{
case Proxy_Config.Redirect_Domain_2_W3.w3_to_domain_permanent:
   app.UseRewriter( new RewriteOptions().AddRedirectToNonWwwPermanent() );
   break;

case Proxy_Config.Redirect_Domain_2_W3.w3_to_domain_temporary:
   app.UseRewriter( new RewriteOptions().AddRedirectToNonWww() );
   break;

case Proxy_Config.Redirect_Domain_2_W3.domain_to_w3_permanent:
   app.UseRewriter( new RewriteOptions().AddRedirectToWwwPermanent() );
   break;

case Proxy_Config.Redirect_Domain_2_W3.domain_to_w3_temporary:
   app.UseRewriter( new RewriteOptions().AddRedirectToWww() );
   break;
}



app.MapReverseProxy();



app.Run();
