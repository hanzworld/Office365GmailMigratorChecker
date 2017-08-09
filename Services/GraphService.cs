using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Office365GmailMigratorChecker
{
    class GraphService
    {
        Graph _settings;

        public GraphService(IOptions<Graph> settings)
        {
            //quick sanity check that we loaded something rather than breaking later!
            if (settings.Value.Username == null)
            {
                throw new Exception("Failed to load configuration settings correctly");
            }

            _settings = settings.Value;
            
        }

        public async Task<List<Message>> RetrieveBatch(DateTime startDate, DateTime endDate)
        {
            if (startDate == null || endDate == null || startDate == DateTime.MinValue || endDate == DateTime.MinValue || endDate <= startDate)
            {
                throw new Exception("I have received some invalid date combinations, please fix me");
            }
            var messages = await GetAllEmailsWithinPeriod(startDate, endDate);
            messages = FilterOutDuplicates(messages);
            return messages;

        }

        private async Task<List<Message>> GetAllEmailsWithinPeriod(DateTime startDate, DateTime endDate)
        {
            GraphServiceClient graphClient = new GraphServiceClient(new AzureAuthenticationProvider(_settings));

            //todo - iterate through every year

            var emailrequest = graphClient.Users[_settings.Username].Messages.Request();

            emailrequest.Filter(String.Format("sentDateTime ge {0} and sentDateTime lt {1}", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd")));
            emailrequest.Select("internetMessageId, createdDateTime, receivedDateTime, sentDateTime, subject, hasAttachments, sender, from, toRecipients, ccRecipients, bccRecipients, isDraft");
            emailrequest.Top(100);
            emailrequest.OrderBy("sentDateTime");

            var batchResults = await emailrequest.GetAsync();

            var messages = new List<Message>(batchResults);

            while (batchResults.NextPageRequest != null)
            {
                batchResults = await batchResults.NextPageRequest.GetAsync();
                Console.WriteLine("{0} : {1}", batchResults[0].SentDateTime, batchResults[0].InternetMessageId);
                messages.AddRange(batchResults);
                Console.WriteLine("Retrieving new batch from Microsoft, now at {0}", messages.Count);
            }
            return messages;
        }

        private List<Message> FilterOutDuplicates(List<Message> messages)
        {
            //here are many many duplicates in Outlook, so we need to get rid of them - we'll only take one from each list
            var filteredmessages = messages.DistinctBy(x => x.InternetMessageId).ToList();
            // testing code for what are the duplicates var duplicates = messages.GroupBy(x => x.InternetMessageId).Where(grp => grp.Count() > 1);
            Console.WriteLine("Ended up with {0} unique messages retrieved", filteredmessages.Count);
            return filteredmessages;

        }


        class AzureAuthenticationProvider : IAuthenticationProvider
        {
            Graph _settings;

            public AzureAuthenticationProvider (Graph settings)
            {
                //quick sanity check that we loaded something rather than breaking later!
                if (settings.Tenant == null)
                {
                    throw new Exception("Failed to load configuration settings correctly or settings missing");
                }
                _settings = settings;
            }

            // Define other methods and classes here
            public async Task AuthenticateRequestAsync(HttpRequestMessage request)
            {                
                //  Constants
                var resource = "https://graph.microsoft.com/";

                string[] _scopes = new string[] { "user.read " };

                //  Ceremony
                var authority = $"https://login.microsoftonline.com/{_settings.Tenant}";
                var authContext = new AuthenticationContext(authority);
                var credentials = new ClientCredential(_settings.ClientId, _settings.Secret);
                var authResult = await authContext.AcquireTokenAsync(resource, credentials);

                request.Headers.Add("Authorization", "Bearer " + authResult.AccessToken);

            }
        }
    }
}
