using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using LegoControl.UI;
using LegoControl.Core.Services;
using LegoControl.UI.Services;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();
builder.Services.AddScoped<ILegoModelService, LocalStorageLegoModelService>();
builder.Services.AddScoped<IDeviceService, LocalStorageDeviceService>();
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<IBluetoothService, WebBluetoothService>();
builder.Services.AddScoped<ILegoHubService, LegoHubService>();

await builder.Build().RunAsync();
