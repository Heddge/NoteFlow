using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElectronNET.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NoteFlow.Helpers;

namespace NoteFlow
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
            {
                string? electronWebPort = ResolveElectronWebPort(args);
                if (!string.IsNullOrWhiteSpace(electronWebPort) &&
                    string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
                {
                    string bindingUrls = ElectronRuntimeUrlHelper.BuildBindingUrls(electronWebPort);
                    Console.WriteLine($"Binding ASP.NET Core to {bindingUrls}");
                    webBuilder.UseUrls(bindingUrls);
                }

                webBuilder.UseElectron(args);
                webBuilder.UseEnvironment("Development");
                webBuilder.UseStartup<Startup>();
            }
        );

        private static string? ResolveElectronWebPort(string[] args)
        {
            foreach (string arg in args)
            {
                if (!arg.StartsWith("/electronWebPort=", StringComparison.OrdinalIgnoreCase) &&
                    !arg.StartsWith("--electronWebPort=", StringComparison.OrdinalIgnoreCase) &&
                    !arg.StartsWith("/electron-web-port=", StringComparison.OrdinalIgnoreCase) &&
                    !arg.StartsWith("--electron-web-port=", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                int equalsIndex = arg.IndexOf('=');
                if (equalsIndex <= 0 || equalsIndex == arg.Length - 1)
                    continue;

                return arg.Substring(equalsIndex + 1);
            }

            return null;
        }
    }

}
/*var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
*/
