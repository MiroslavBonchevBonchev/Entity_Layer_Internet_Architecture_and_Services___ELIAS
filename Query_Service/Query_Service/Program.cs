using Data_Access;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Query_Service.Client.Pages;
using Query_Service.Components;
using Query_Service.Components.Account;
using Query_Service.Data;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication( options =>
    {
       // Add other authentication providers, like social networks accounts, here somewhere.
       options.DefaultScheme = IdentityConstants.ApplicationScheme;
       options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    } )
    .AddIdentityCookies();


var connectionString = builder.Configuration.GetConnectionString("QS_UserDB_Connection") ?? throw new InvalidOperationException( "Connection string 'QS_UserDB_Connection' not found." );

// todo add configuration and install selection
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

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>( options => options.SignIn.RequireConfirmedAccount = true )
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddStackExchangeRedisCache( options =>
{
   options.Configuration = builder.Configuration.GetConnectionString( "CacheRedis" );
   options.InstanceName  = "elias_QS_";
} );

builder.Services.AddSingleton< IEmailSender< ApplicationUser >, IdentityNoOpEmailSender >();
builder.Services.AddSingleton< IDBExecute, DBExecute >();
builder.Services.AddSingleton< ITemperatureData, TemperatureData >();

var app = builder.Build();

// Configure the HTTP request pipeline.
if( app.Environment.IsDevelopment() )
{
   app.UseWebAssemblyDebugging();
   app.UseMigrationsEndPoint();
}
else
{
   app.UseExceptionHandler( "/Error", createScopeForErrors: true );
   // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
   app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies( typeof( Query_Service.Client._Imports ).Assembly );

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
