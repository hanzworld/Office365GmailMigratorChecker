using Microsoft.Graph;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Office365GmailMigratorChecker
{
    class Application
    {

        public Application(GmailService gmailService, GraphService graphService, DataStoreService dataStoreService, IOptions<AppSettings> settings)
        {
            _gmailService = gmailService;
            _graphService = graphService;
            _dataStoreService = dataStoreService;
            _settings = settings.Value;

        }

        const int startYear = 2013;
        const int period = 3;
        private GmailService _gmailService;
        private GraphService _graphService;
        private AppSettings _settings;
        private DataStoreService _dataStoreService;

        public async Task Run()
        {
            try
            {
                // STEP 1: Retrieve a list of messages from Office365 (as the 'original' mail server, it's the source of truth of what should be migrated)
                var messages = new List<MyMessage>();
                //because I'm completely lazy for now, I'm going to store results locally in JSON files - this might bite me later, but at least it'll help me write the app without constant API thrashing
                if (LocalPersistanceService.LocalFileExists(_settings.StartYear, _settings.Periods, _settings.PeriodLength))
                {
                    messages = LocalPersistanceService.ReadResultsFromFile(_settings.StartYear, _settings.Periods, _settings.PeriodLength);
                }
                else
                {
                    //get them from the API
                    var outlookData = await _graphService.RetrieveData(_settings.StartYear, _settings.Periods);
                    messages = FilterOutDuplicates(outlookData);
                    LocalPersistanceService.PersistResultsToFile(messages, _settings.StartYear, _settings.Periods, _settings.PeriodLength);
                }
               
                //STEP 2: find if these have been imported to Gmail - where the only matching criteria is RFC822 MessageID
                //TODO given we have to make n calls to Gmail API, one for each message, let's at least batch them shall we?

                var searchRequest = _gmailService.Users.Messages.List("[USERNAME]");
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

                    
                }

                var missingMessages = messages.Where(m => !m.isInGmail).ToList();

                // STEP 3: Where we have messages which are not migrated, we need to store those
                _dataStoreService.WriteToDb(missingMessages);


                Console.WriteLine(missingMessages.Count);
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
