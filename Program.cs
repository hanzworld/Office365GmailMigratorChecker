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

            Task.Run(() => app.Run()).GetAwaiter().GetResult();

            Console.ReadKey();
        }
       

        private static void ConfigureServices(IServiceCollection services)
        {
        }





        }



    }
}