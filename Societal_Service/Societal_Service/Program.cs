using Societal_Service.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Enable Controllers, MBB
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if( !app.Environment.IsDevelopment() )
{
   app.UseExceptionHandler( "/Error", createScopeForErrors: true );
   // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
   app.UseHsts();
}



// Require HTTPS conditionally, MBB.
if( !string.IsNullOrWhiteSpace( Environment.GetEnvironmentVariable( "FORCE_HTTPS_BEHING_GATEWAY" ) ) )
{
   string force_https_behing_gateway = Environment.GetEnvironmentVariable( "FORCE_HTTPS_BEHING_GATEWAY" ) ?? "";

   if( 0 != string.Compare( force_https_behing_gateway, "NO", true ) )
   {
      if( 0 == string.Compare( force_https_behing_gateway, "YES", true ) )
      {
         app.UseHttpsRedirection();
      }
      else
      {
         throw new Exception( "Invalid value for FORCE_HTTPS_BEHING_GATEWAY environment variable. Must be YES, NO or omitted altogether defaulting to NO." );
      }
   }
}



app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


// Enable Controllers, MBB
app.MapControllers();
app.MapControllerRoute( "default", "" );

app.Run();
