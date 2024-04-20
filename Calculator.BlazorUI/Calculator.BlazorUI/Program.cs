using Calculator.BlazorUI.Client.Pages;
using Calculator.BlazorUI.Components;
using Calculator.UI;
using Calculator.UI.Abstractions;
using Calculator.UI.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSingleton<ICalculatorUi>((_) =>
{
    var booter = new Bootstrapper();
    return booter.Initialize();
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Calculator.BlazorUI.Client._Imports).Assembly);

app.Run();
