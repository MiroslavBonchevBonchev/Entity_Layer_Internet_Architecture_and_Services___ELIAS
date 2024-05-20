using Settings;
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
if( Launch_settings.Force_HTTPS_on_ELIAS_service( Launch_settings.Protocol_permission.Required, Launch_settings.Protocol_permission.Banned, true ) )
{
   app.UseHttpsRedirection();
}



app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


// Enable Controllers, MBB
app.MapControllers();
app.MapControllerRoute( "default", "" );

app.Run();
