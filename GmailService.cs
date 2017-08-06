using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using OfficialGmailService = Google.Apis.Gmail.v1.GmailService;

namespace Office365GmailMigratorChecker
{
    class GmailService : OfficialGmailService
    {
        private Gmail _settings;

        public GmailService(IOptions<Gmail> settings) : base(ConstructBaseInitializer()) {
            //quick sanity check that we loaded something rather than breaking later!
            if (settings.Value.Username == null)
            {
                throw new Exception("Failed to load configuration settings correctly");
            }
            _settings = settings.Value;
        }
               

        private static Initializer ConstructBaseInitializer()
        {
            UserCredential credential;
            string[] Scopes = { Scope.GmailReadonly };

            using (var stream = new FileStream(@"OutlookGmailComparer.Gmail.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = Environment.GetEnvironmentVariable(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "LocalAppData" : "Home");
                credPath = Path.Combine(credPath, ".credentials/barrowside-email-management.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }


            return new Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Email De-Duplicator"
            };
        }         
    }
}
