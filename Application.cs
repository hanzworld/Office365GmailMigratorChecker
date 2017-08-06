using Microsoft.Graph;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Office365GmailMigratorChecker
{
    class Application
    {

        public Application(GmailService gmailService, GraphService graphService)
        {
            _gmailService = gmailService;
            _graphService = graphService;

        }

        const int startYear = 2013;
        const int period = 3;
        private GmailService _gmailService;
        private GraphService _graphService;

        public async Task Run()
        {
            try
            {
                var outlookData = await _graphService.RetrieveData(startYear, period);
                var messages = FilterOutDuplicates(outlookData);
                LocalPersistanceService.PersistResultsToFile(messages);

                //because I'm completely lazy for now, I'm going to add them to JSON files - this might bite me later, but at least it'll help me write the app without constant testing
                //System.IO.File.WriteAllText(@"D:\Hanz\Dropbox\Coding\Office365DataStore.json", JsonConvert.SerializeObject(messages));
                // var db = InstantiateDataStore();
                // var keyFactory = db.CreateKeyFactory("Message");

                // var messages = ReadResultsFromFile();
                //now we want to find if these have been imported to Gmail - where the only matching criteria is RFC822 MessageID
                //TODO given we have to make n calls to Gmail API, one for each message, let's at least batch them shall we?

                var searchRequest = _gmailService.Instance.Users.Messages.List("[USERNAME]");
                searchRequest.IncludeSpamTrash = true;
                searchRequest.MaxResults = 1;

                int i = 0;
                var errors = new List<string>();
                foreach (var message in messages)
                {
                    try
                    {
                        searchRequest.Q = String.Format("rfc822msgid:{0}", message.outlookMessage.InternetMessageId.Replace("<", "").Replace(">", ""));

                        var result = searchRequest.Execute();
                        if (result.Messages != null)
                        {
                            message.isInGmail = true;
                            message.gmailId = result.Messages[0].Id;
                        }
                    }
                    catch (Exception e)
                    {
                        errors.Add(String.Format("Couldn't retrieve id {0} from Gmail (date: {1}, subject: {2}). Error was {3}", message.outlookMessage.InternetMessageId, message.outlookMessage.SentDateTime, message.outlookMessage.Subject, e.Message));
                    }
                    finally
                    {
                        i++;
                        if (i % 100 == 0)
                        {
                            Console.WriteLine("Now at {0}00", i / 100);
                        }
                    }

                    // WriteToDb(db, keyFactory, message);
                }

                Console.WriteLine(messages.Count(x => !x.isInGmail));
                //TODO: These are the ones we want to hold onto and persist somewhere that's queryable over and over
            }
            catch (Exception e)
            {

            }
        }

        public static List<MyMessage> FilterOutDuplicates(List<Message> messages)
        {
            //here are many many duplicates in Outlook, so we need to get rid of them - we'll only take one from each list
            var filteredmessages = messages.DistinctBy(x => x.InternetMessageId).Select(x => new MyMessage() { outlookMessage = x }).ToList();
            // testing code for what are the duplicates var duplicates = messages.GroupBy(x => x.InternetMessageId).Where(grp => grp.Count() > 1);
            Console.WriteLine("Ended up with {0}", filteredmessages.Count);
            return filteredmessages;

        }
    }
}
