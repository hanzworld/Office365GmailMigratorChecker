using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graph;
using MoreLinq;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace Office365GmailMigratorChecker
{
    class Program
    {

        static void Main()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            // Application application = new Application(serviceCollection);
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

           var app = serviceProvider.GetService<Application>();

            //Task.Run(() => app.Run()).Wait();
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