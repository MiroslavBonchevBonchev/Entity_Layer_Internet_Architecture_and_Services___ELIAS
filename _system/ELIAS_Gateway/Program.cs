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
   .LoadFromConfig( builder.Configuration.GetSection( "ReverseProxy" ) );

var app = builder.Build();

if( app.Environment.IsDevelopment() )
{
   app.UseSwagger();
   app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapReverseProxy();

app.Run();
