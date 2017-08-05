using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using OfficialGmailService = Google.Apis.Gmail.v1.GmailService;

namespace Office365GmailMigratorChecker
{
    static class GmailService
    {
        
        private static OfficialGmailService _instance = new OfficialGmailService(ConstructBaseInitializer());
        public static OfficialGmailService Instance { get { return _instance; } }
        

        private static BaseClientService.Initializer ConstructBaseInitializer()
        {
            Google.Apis.Auth.OAuth2.UserCredential credential;
            string[] Scopes = { OfficialGmailService.Scope.GmailReadonly };

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


            return new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Email De-Duplicator"
            };
        }         
    }
}
