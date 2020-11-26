using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Code.Hub.App.Startups
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var settingsFileName = $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json";
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(settingsFileName, optional: false, reloadOnChange: true)
                .Build();

            var urlsFromConfig = config.GetValue("Application:HostedOnUrls", string.Empty);
            var urls = string.IsNullOrEmpty(urlsFromConfig) ? new string[0] : urlsFromConfig.Split(";", StringSplitOptions.RemoveEmptyEntries);

            CreateHostBuilder(urls, args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] urls, string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel(opt => opt.AddServerHeader = false);
                    webBuilder.UseUrls(urls);
                    webBuilder.UseContentRoot(Directory.GetCurrentDirectory());
                    webBuilder.ConfigureLogging(builder =>
                    {
                        builder.AddFilter("Microsoft", LogLevel.Warning)
                            .AddFilter("System", LogLevel.Warning);
                    });
                    webBuilder.UseIIS();
                    webBuilder.UseIISIntegration();
                    webBuilder.UseStartup<Startup>();
                });
    }
}
