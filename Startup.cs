using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Office365GmailMigratorChecker
{
    class Startup
    {

        static void Main()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

           var app = serviceProvider.GetService<Application>();

            Task.Run(() => app.Run()).GetAwaiter().GetResult();

            Console.ReadKey();
        }
       

        private static void ConfigureServices(IServiceCollection services)
        {
            //add program configuration
            IConfigurationRoot configuration = GetConfiguration();
            services.AddSingleton<IConfigurationRoot>(configuration);

            //separate my configuration settings please
            services.AddOptions();
            services.Configure<Gmail>(options => configuration.GetSection("Gmail").Bind(options));
            services.Configure<Graph>(options => configuration.GetSection("Graph").Bind(options));
            services.Configure<AppSettings>(options => configuration.Bind(options));

            //and then my actual classes
            services.AddTransient<Application>();
            services.AddSingleton<GmailService>();
            services.AddSingleton<GraphService>();
            services.AddSingleton<DataStoreService>();

            ILoggerFactory loggerFactory = new LoggerFactory()
                .AddConsole()
                .AddDebug();

            services.AddSingleton(loggerFactory); // Add first my already configured instance
            services.AddLogging(); // Allow ILogger<T>*/

        }

        private static IConfigurationRoot GetConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange:true)
                .Build();
        }



    }
}