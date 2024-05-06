using Asp.Versioning;
using Asp.Versioning.Conventions;
using Data_Access;
using Query_Service.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Query_Service.Client.Pages;
using Query_Service.Components;
using Query_Service.Components.Account;
using Query_Service.Data;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Query_Service.Swagger;
using Grpc.Net.Client;
using Query_Service.Protos;
using MassTransit;
using Microsoft.AspNetCore.HttpOverrides;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();



// Consume request properties from headers when behind gateway - step 1/2, MBB
builder.Services.Configure< ForwardedHeadersOptions >( options =>
{
   options.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedPrefix;
   options.KnownNetworks.Clear();
   options.KnownProxies.Clear();
} );



builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped< IdentityUserAccessor >();
builder.Services.AddScoped< IdentityRedirectManager >();
builder.Services.AddScoped< AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider >();

builder.Services.AddAuthentication( options =>
{
   // Add other authentication providers, like social networks accounts, here somewhere.
   options.DefaultScheme       = IdentityConstants.ApplicationScheme;
   options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
} ).AddIdentityCookies();



// Enable Controllers, MBB
builder.Services.AddControllers();



// Enable Authorization, MBB
builder.Services.AddAuthorization( /*options => { Add policies }*/ );



// Add API Versions, MBB
builder.Services.AddApiVersioning( options =>
{
   options.DefaultApiVersion = new ApiVersion( majorVersion: 1, minorVersion: 0 );
   options.AssumeDefaultVersionWhenUnspecified = true;
   options.ReportApiVersions = true;
   options.ApiVersionReader = new HeaderApiVersionReader( "elias_qs-api-version" );
} ).AddApiExplorer();



// Swagger - start x 1/2 places, MBB
// Todo probably want to remove swagger
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient< IConfigureOptions< SwaggerGenOptions >, ConfigureSwaggerOptions >();
builder.Services.AddSwaggerGen( optiuons => optiuons.OperationFilter< SwaggerDefaultValues >() );
// Swagger - end



var connectionString = builder.Configuration.GetConnectionString("QS_UserDB_Connection") ?? throw new InvalidOperationException( "Connection string 'QS_UserDB_Connection' not found." );

// todo - add configuration and install selection for the database type
DB_provider db_provider = DB_provider.MySQL;

switch( db_provider )
{
   case DB_provider.MySQL:
   builder.Services.AddDbContext<ApplicationDbContext>( options => options.UseMySql( connectionString, new MySqlServerVersion( new Version( 8, 0, 35 ) ) ) );
      break;
   case DB_provider.MSSQL:
      builder.Services.AddDbContext<ApplicationDbContext>( options => options.UseSqlServer( connectionString ) );
      break;
   case DB_provider.PostGRE:
      for( int i = 0; i < 10; i++ ) { Console.WriteLine( "DB_provider.PostGRE NOT INITIALIZED = Program.cs todo" ); }
      break;
}



// Add Redis Cache
builder.Services.AddStackExchangeRedisCache( options =>
{
   options.Configuration = builder.Configuration.GetConnectionString( "CacheRedis" );
   options.InstanceName  = "elias_QS_";
} );



// Add message broker and manager, MBB
builder.Services.AddMassTransit( bus_configurator =>
{
   bus_configurator.SetSnakeCaseEndpointNameFormatter();

   bus_configurator.AddConsumer< TestEventRabbitMQCaptureResponse >();

   bus_configurator.UsingRabbitMq( (context, configurator) =>
   {
      configurator.Host( new Uri( builder.Configuration["MessageBroker:Host"]! ), host =>
      {
         host.Username( builder.Configuration["MessageBroker:Username"] );
         host.Password( builder.Configuration["MessageBroker:Password"] );
      } );

      configurator.ConfigureEndpoints( context );
   } );
} );



// App Management
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddIdentityCore<ApplicationUser>( options => options.SignIn.RequireConfirmedAccount = true )
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();



// App Services
builder.Services.AddSingleton< IEmailSender< ApplicationUser >, IdentityNoOpEmailSender >();



// App services, MBB
builder.Services.AddSingleton< IDBExecute, DBExecute >();
builder.Services.AddSingleton< ITemperatureData, TemperatureData >();



// Enable gRPC, MBB
builder.Services.AddGrpc();



var app = builder.Build();



// Consume request properties from headers when behind gateway - step 2/2, MBB
app.UseForwardedHeaders();



// Add API Versions, MBB
app.NewApiVersionSet()
      .HasApiVersion( 1, 0 )
      .HasApiVersion( 2, 0 )
      .ReportApiVersions()
      .Build();



// Configure the HTTP request pipeline.
if( app.Environment.IsDevelopment() )
{
   app.UseWebAssemblyDebugging();
   app.UseMigrationsEndPoint();

   // Swagger - start x 2/2 places, MBB
   app.UseSwagger();
   app.UseSwaggerUI( o =>
   {
      foreach( var description in app.DescribeApiVersions() )
      {
         var url = $"/swagger/{description.GroupName}/swagger.json";
         var name = description.GroupName.ToUpperInvariant();
         o.SwaggerEndpoint( url, name );
      }
   } );
   // Swagger - end

   app.UseDeveloperExceptionPage();
}
else
{
   app.UseExceptionHandler( "/Error", createScopeForErrors: true );

   // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
   app.UseHsts();
}





bool todo_Query_Service_is_behing_gateway = true;
if( !todo_Query_Service_is_behing_gateway )
{
   app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies( typeof( Query_Service.Client._Imports ).Assembly );

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();



// Enable Controllers, MBB
app.MapControllers();
app.MapControllerRoute("default", "");



// Enable gRPC, MBB
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapGrpcService<test_GreetServiceClass>();
app.MapGet( "/gprc", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909" );

// Usage
//    Console.WriteLine( "Hello, World!" );
//    var channel = GrpcChannel.ForAddress( "https://localhost:[https port]/gprc");
//    var client = new test_greeter.test_greeterClient( channel );
//    var input = new test_HelloRequest { Name = "John" };
//    var reply = await client.SayHelloAsync( input );
//    Console.WriteLine( reply.Message );
// NuGet
//    google.protobuf
//    grpc.net.client
//    grpc.tools



// rabbit MQ



app.Run();
