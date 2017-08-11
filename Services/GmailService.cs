using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Logging;
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
        private ILogger<GmailService> _logger;

        public GmailService(IOptions<Gmail> settings, ILogger<GmailService> logger) : base(ConstructBaseInitializer()) {
            //quick sanity check that we loaded something rather than breaking later!
            if (settings.Value.Username == null)
            {
                throw new Exception("Failed to load configuration settings correctly");
            }
            _settings = settings.Value;
            _logger = logger;
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

        public string FindMessageByRFC822(string rfc822MessageId){

            //TODO: allow other usernames, as stored in _settings.Username
            var searchRequest = Users.Messages.List("me");
            searchRequest.IncludeSpamTrash = true;
            searchRequest.MaxResults = 1;

            searchRequest.Q = String.Format("rfc822msgid:{0}", rfc822MessageId);

            var result = searchRequest.Execute();

            if (result.Messages != null)
            {
                return result.Messages[0].Id;
            }
            return null;
        }
    }
}
