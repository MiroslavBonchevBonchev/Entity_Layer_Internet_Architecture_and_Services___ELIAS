using ELIAS_Gateway.Configuration;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Transforms;

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
   .LoadFromMemory( Proxy_Config.Get_routes(), Proxy_Config.Get_clusters() );


//// Add LettuceEncrypt when the HTTPS protocol is required, MBB
//string lets_encrypt_certificate_email = Proxy_Config.Get_TLS_CERT_EMAIL();
//var    lets_encrypt_domain_uris       = Proxy_Config.Get_domains_for_TLS();

//if( !string.IsNullOrWhiteSpace( lets_encrypt_certificate_email ) && 0 != lets_encrypt_domain_uris.Count )
//{
//   builder.Services.AddLettuceEncrypt();

//   builder.Services.AddLettuceEncrypt( context =>
//   {
//      context.AcceptTermsOfService = true;
//      context.EmailAddress = lets_encrypt_certificate_email;
//      context.DomainNames  = [.. lets_encrypt_domain_uris];
//   } );
//}


var app = builder.Build();

if( app.Environment.IsDevelopment() )
{
   foreach( var route in Proxy_Config.Get_routes() )
   {
      Console.WriteLine( $"RouteId: {route.RouteId} - ClusterId : {route.ClusterId} - Addresses: {string.Join( ", ", null != route.Match ? route.Match.Hosts : "" )}"  );
   }
   
   foreach( var cluster in Proxy_Config.Get_clusters() )
   {
      Console.WriteLine( $"ClusterId : {cluster.ClusterId} - Addresses: {string.Join( ", ", null != cluster.Destinations ? cluster.Destinations.Keys : "" )}"  );
   }

   foreach( var domains in Proxy_Config.Get_domains_for_TLS() )
   {
      Console.WriteLine( $"TLS Domains : {string.Join( ", ", domains )}"  );
   }

   app.UseSwagger();
   app.UseSwaggerUI();
}


//// Require HTTPS only when the HTTPS protocol is required.
//if( !string.IsNullOrWhiteSpace( lets_encrypt_certificate_email ) )
//{
//   app.UseHttpsRedirection();
//}
app.MapReverseProxy();

app.Run();
