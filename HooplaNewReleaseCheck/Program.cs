using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HooplaNewReleaseCheck
{
    class Program
    {
        static async Task Main()
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Build())
                .CreateLogger();

            Log.Logger.Information("Application Starting");

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<IHooplaResponse, HooplaResponse>();
                    services.AddTransient<IEmail, Email>();
                })
                .UseSerilog()
                .Build();

            // TODO: Create a list of titles and authors that I am interested in checking up on.  This list could live in a database, json, Google sheet???? or hardcoded.
            // TODO: Maybe save results to Db.  The would have a null Borrowed dateTime.  This could get updated once the book has been borrowed and read.

            try
            {
                var svc = ActivatorUtilities.CreateInstance<HooplaResponse>(host.Services);
                List<DigitalBook> newBooksToRead = await svc.Run();

                if (newBooksToRead != null)
                {
                    newBooksToRead.Sort();
                    var eSvc = ActivatorUtilities.CreateInstance<Email>(host.Services);
                    await eSvc.SendEmailAsync(newBooksToRead);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables();
        }
    }
}
