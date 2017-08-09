using Microsoft.Graph;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Options;
using Office365GmailMigratorChecker.Model;

namespace Office365GmailMigratorChecker
{
    class Application
    {

        public Application(GmailService gmailService, GraphService graphService, SqlExpressService dataStoreService, IOptions<AppSettings> settings)
        {
            //quick sanity check that we loaded something rather than breaking later!
            if (settings.Value.StartYear == 0)
            {
                throw new Exception("Failed to load configuration settings correctly");
            }

            _gmailService = gmailService;
            _graphService = graphService;
            _dataStoreService = dataStoreService;
            _settings = settings.Value;

        }

        private GmailService _gmailService;
        private GraphService _graphService;
        private AppSettings _settings;
        private SqlExpressService _dataStoreService;

        public async Task Run()
        {
            //TODO start with the dates in the settings, then iterate forrward the specific length of time, putting this in a for loop

            var startDate = new DateTime(_settings.StartYear, 1, 1);
            //calculate end date
            DateTime endDate = startDate;
            switch (_settings.PeriodLength)
            {
                case PeriodType.Year:
                    endDate = startDate.AddYears(_settings.Periods);
                    break ;
                case PeriodType.Month:
                    endDate = startDate.AddMonths(_settings.Periods);
                    break;
            }
            
            var messageBatch = new MessageBatch(startDate, endDate);

            try
            {
                // STEP 1: Retrieve a list of messages from Office365 (as the 'original' mail server, it's the source of truth of what should be migrated)
                messageBatch = await GetOutlookData(messageBatch);

                //STEP 2: find if these have been imported to Gmail - where the only matching criteria is RFC822 MessageID
                messageBatch = MatchToGmailData(messageBatch);

                // STEP 3: Where we have messages which are not migrated, we need to store those so it's queryable over and over
                _dataStoreService.WriteToDb(messageBatch.NotMigratedMessages);

                //STEP 4: Where we have messages we simply can't work out, store them to work on later
                _dataStoreService.WriteToDb(messageBatch.UnconfirmedMigrationStatus);

                Console.WriteLine("Complete");
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e}");
            }
            finally
            {
                //always save wherever we got to so I don't have to keep rehitting the APIs again
                LocalPersistanceService.PersistResultsToFile(messageBatch);
            }
        }
        
        private MessageBatch MatchToGmailData(MessageBatch messageBatch)
        {
            //TODO given we have to make n calls to Gmail API, one for each message, let's at least batch them shall we?

            //because we may have read out a partially completed file, dont' assume we need to reparse everything!
            var howManyMessagesAreAlreadyProcessed = messageBatch.ConfirmedMigrationStatus.Count;
            if (howManyMessagesAreAlreadyProcessed > 1)
            {
                Console.WriteLine("{0} of the loaded message were already processed, quering Gmail API for {1} remaining messages", howManyMessagesAreAlreadyProcessed, messageBatch.Messages.Count - howManyMessagesAreAlreadyProcessed);
            }

            int i = 0;
            var errors = new List<string>();
            foreach (var message in messageBatch.UnconfirmedMigrationStatus)
            {
                try
                {
                    var gmailId = _gmailService.FindMessageByRFC822(message.OutlookMessage.InternetMessageId.Replace("<", "").Replace(">", ""));
                    bool isInGmail = !String.IsNullOrWhiteSpace(gmailId);

                    message.IsMigratedToGmail = isInGmail;

                    if (isInGmail)
                    {
                        message.GmailId = gmailId;
                    }
                }
                catch (Exception e)
                {
                    errors.Add(String.Format("Couldn't retrieve id {0} from Gmail (date: {1}, subject: {2}). Error was {3}", message.OutlookMessage.InternetMessageId, message.OutlookMessage.SentDateTime, message.OutlookMessage.Subject, e.Message));
                }
                finally
                {
                    i++;
                    if (i % 100 == 0)
                    {
                        Console.WriteLine("Retriving Gmail information for messages {0}00 and onwards", i / 100);
                    }
                }


            }
            Console.WriteLine(messageBatch);

            LocalPersistanceService.PersistResultsToFile(messageBatch);
            return messageBatch;
        }

        async Task<MessageBatch> GetOutlookData(MessageBatch messageBatch)
        {
            //because I'm completely lazy for now, I'm going to store results locally in JSON files - this might bite me later, but at least it'll help me write the app without constant API thrashing
            if (LocalPersistanceService.LocalFileExists(messageBatch))
            {
                messageBatch = LocalPersistanceService.ReadResultsFromFile(messageBatch);
            }
            else
            {
                //get them from the API
                var outlookData = await _graphService.RetrieveBatch(messageBatch.StartDate, messageBatch.EndDate);
                if (outlookData.Count == 0) return null;
                //convert them into a data format we actually can use, and persist

                //TODO - put this in a proper converter
                messageBatch.Messages = outlookData.Select(m => new MyMessage { OutlookMessage = m }).ToList();

                LocalPersistanceService.PersistResultsToFile(messageBatch);
            }
            return messageBatch;

        }


    }
}
