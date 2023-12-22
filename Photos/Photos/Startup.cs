using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Photos.AnalyzerService;
using Photos.AnalyzerService.Abstraction;

[assembly: FunctionsStartup(typeof(Photos.Startup))]

namespace Photos
{
    internal class Startup : FunctionsStartup
    {
        //public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        //{
        //    // Set up configuration sources
        //    //builder.ConfigurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        //    builder.ConfigurationBuilder.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
        //    builder.ConfigurationBuilder.AddEnvironmentVariables();
        //}

        public override void Configure(IFunctionsHostBuilder builder)
        {
            //var versionKey = 
            builder.Services.AddSingleton<IAnalyzerService, ComputerVisionAnalyzerService>();
        }
    }
}
