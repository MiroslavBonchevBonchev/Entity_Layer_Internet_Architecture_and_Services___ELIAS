using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection;
using Settings;
using Societal_Service.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Enable Controllers, MBB
builder.Services.AddControllers();



// Prevent WARNING: Storing keys in a directory '/home/app/.aspnet/DataProtection-Keys' that may not be persisted outside of the container. MBB
builder.Services.AddDataProtection()
   .PersistKeysToFileSystem( new DirectoryInfo( $"/home/app/data_protection_keys" ) )
   .UseCryptographicAlgorithms( new AuthenticatedEncryptorConfiguration()
   {
      EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
      ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
   } );



var app = builder.Build();

// Configure the HTTP request pipeline.
if( !app.Environment.IsDevelopment() )
{
   app.UseExceptionHandler( "/Error", createScopeForErrors: true );
   // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
   app.UseHsts();
}



// This service works with HTTP behind reverse proxy, MBB.
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
