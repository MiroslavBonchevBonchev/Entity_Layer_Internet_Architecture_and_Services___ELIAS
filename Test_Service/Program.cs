using MassTransit;
using Test_Service.Components;
using Test_Service.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();



// Add message broker and manager, MBB
builder.Services.AddMassTransit( bus_configurator =>
{
   bus_configurator.SetSnakeCaseEndpointNameFormatter();

   bus_configurator.AddConsumer< TestEvent1Consumer >();

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



var app = builder.Build();

// Configure the HTTP request pipeline.
if( !app.Environment.IsDevelopment() )
{
   app.UseExceptionHandler( "/Error", createScopeForErrors: true );
   // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
   app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
