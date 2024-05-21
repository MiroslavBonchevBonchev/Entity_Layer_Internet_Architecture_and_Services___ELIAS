using ELIAS_Gateway.Configuration;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Settings;
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



// Add LettuceEncrypt when the HTTPS protocol is required, MBB
string lets_encrypt_certificate_email = Proxy_Config.Get_TLS_CERT_EMAIL();

if( !string.IsNullOrWhiteSpace( lets_encrypt_certificate_email ) )
{
   builder.Services.AddLettuceEncrypt();

   builder.Services.AddLettuceEncrypt( context =>
   {
      context.AcceptTermsOfService = true;
      context.EmailAddress = lets_encrypt_certificate_email;
      context.DomainNames  = [.. Proxy_Config.Get_domains_for_TLS()];
   } );
}



var app = builder.Build();

if( app.Environment.IsDevelopment() )
{
   foreach( var route in Proxy_Config.Get_routes() )
   {
      Console.WriteLine( $"RouteId: {route.RouteId} - ClusterId : {route.ClusterId} - Addresses: {string.Join( ", ", null != route.Match && null != route.Match.Hosts ? route.Match.Hosts : new List< string >() )}"  );
   }
   
   foreach( var cluster in Proxy_Config.Get_clusters() )
   {
      Console.WriteLine( $"ClusterId : {cluster.ClusterId} - Addresses: {(null != cluster.Destinations && 0 != cluster.Destinations.Count ? cluster.Destinations[cluster.Destinations.Keys.First()].Address : "") }" );
   }

   foreach( var domain in Proxy_Config.Get_domains_for_TLS() )
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



app.MapReverseProxy();



app.Run();
