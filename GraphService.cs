using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Office365GmailMigratorChecker
{
    class GraphService
    {
        public static async Task<List<Message>> RetrieveData(int startYear, int period, string username)
        {
            //TODO Need to cache the results so I stop querying the API - or chuck them in a DB?

            GraphServiceClient graphClient = new GraphServiceClient(new AzureAuthenticationProvider());

            DateTime startDate = new DateTime(startYear, 1, 1);
            DateTime endDate = startDate.AddMonths(period);

            //todo - iterate through every year

            var emailrequest = graphClient.Users[username].Messages.Request();

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
                Console.WriteLine("New batch, now {0}", messages.Count);
            }
            return messages;

        }


        class AzureAuthenticationProvider : IAuthenticationProvider
        {

            // Define other methods and classes here
            public async Task AuthenticateRequestAsync(HttpRequestMessage request)
            {
                //  Constants
                var tenant = "barrowside.onmicrosoft.com";
                var resource = "https://graph.microsoft.com/";

                var clientID = "[CLIENT_ID]";
                var secret = "[CLIENT_SECRET]";

                string[] _scopes = new string[] { "user.read, " };

                //  Ceremony
                var authority = $"https://login.microsoftonline.com/{tenant}";
                var authContext = new AuthenticationContext(authority);
                var credentials = new ClientCredential(clientID, secret);
                var authResult = await authContext.AcquireTokenAsync(resource, credentials);

                request.Headers.Add("Authorization", "Bearer " + authResult.AccessToken);

        }
    }
}
