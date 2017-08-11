using Microsoft.Graph;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Options;
using Office365GmailMigratorChecker.Model;
using Microsoft.Extensions.Logging;

namespace Office365GmailMigratorChecker
{
    class Application
    {

        public Application(GmailService gmailService, GraphService graphService, SqlExpressService dataStoreService, IOptions<AppSettings> settings, ILogger<Application> logger, MessageBatchFactory batchFactory)
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
            _logger = logger;
            _messageBatchFactory = batchFactory;

        }

        private GmailService _gmailService;
        private GraphService _graphService;
        private AppSettings _settings;
        private SqlExpressService _dataStoreService;
        private ILogger<Application> _logger;
        private MessageBatchFactory _messageBatchFactory;

        public async Task Run()
        {
            //TODO start with the dates in the settings, then iterate forrward the specific length of time, putting this in a for loop
            for (var startDate = new DateTime(_settings.StartYear, 1, 1); startDate < DateTime.Now; startDate = startDate.JumpAheadBy(_settings.Periods, _settings.PeriodLength))
            {
                DateTime endDate = startDate.JumpAheadBy(_settings.Periods, _settings.PeriodLength);

                _logger.LogInformation($"Starting processing batch between {startDate}, ending {endDate}");
                var messageBatch = _messageBatchFactory.SetupBatch(startDate, endDate);
                
                try
                {

                    // STEP 1: Retrieve a list of messages from Office365 (as the 'original' mail server, it's the source of truth of what should be migrated)
                    messageBatch = await GetOutlookDataAsync(messageBatch);

                    if (messageBatch.Messages == null || messageBatch.Messages.Count == 0)
                    {
                        _logger.LogDebug("No records found"); continue;
                    }

                    //STEP 2: find if these have been imported to Gmail - where the only matching criteria is RFC822 MessageID
                    messageBatch = MatchToGmailData(messageBatch);

                    // STEP 3: Where we have messages which are not migrated, we need to store those so it's queryable over and over
                    _dataStoreService.WriteToDb(messageBatch.NotMigratedMessages);

                    //STEP 4: Where we have messages we simply can't work out, store them to work on later
                    _dataStoreService.WriteToDb(messageBatch.UnconfirmedMigrationStatus);

                    messageBatch.Finish();

                    _logger.LogInformation("Batch complete");
                    startDate = endDate;
                }
                catch (Exception e)
                {
                    _logger.LogError($"ERROR: {e}");
                    //always save wherever we got to so I don't have to keep rehitting the APIs again
                    messageBatch.Save();
                    break;
                }
            }

            _logger.LogInformation("Complete");
        }
        
        private MessageBatch MatchToGmailData(MessageBatch messageBatch)
        {
            //TODO given we have to make n calls to Gmail API, one for each message, let's at least batch them shall we?
            
            int i = 0;
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
                    _logger.LogWarning(String.Format("Couldn't retrieve id {0} from Gmail (date: {1}, subject: {2}). Error was {3}", message.OutlookMessage.InternetMessageId, message.OutlookMessage.SentDateTime, message.OutlookMessage.Subject, e.Message));
                }
                finally
                {
                    i++;
                    if (i % 100 == 0)
                    {
                        _logger.LogDebug("Retriving Gmail information for messages {0}00 and onwards", i / 100);
                    }
                }


            }
           _logger.LogInformation(messageBatch.ToString());
            return messageBatch;
        }

        async Task<MessageBatch> GetOutlookDataAsync(MessageBatch messageBatch)
        {
            
            if (!messageBatch.RetrievedFromCache)
            {
                //get them from the API
                var outlookData = await _graphService.RetrieveBatch(messageBatch.StartDate, messageBatch.EndDate);
                if (outlookData.Count == 0) return messageBatch;
                //convert them into a data format we actually can use, and persist

                //TODO - put this in a proper converter
                messageBatch.Messages = outlookData.Select(m => new MyMessage { OutlookMessage = m }).ToList();
                _logger.LogInformation($"Retrieved {outlookData.Count} messages from Microsoft");

                messageBatch.Save();
            }
             else
            {
                //TODO implement a way to disable automatically loading from a cache
                _logger.LogDebug("Loaded this batch from a previous file. Turn off caching if you didn't want this");

                var howManyMessagesAreAlreadyProcessed = messageBatch.ConfirmedMigrationStatus.Count;
                if (howManyMessagesAreAlreadyProcessed > 1)
                {
                    _logger.LogInformation("Found {0} of these message have already been processed, leaving {1} messages to find information on", howManyMessagesAreAlreadyProcessed, messageBatch.Messages.Count - howManyMessagesAreAlreadyProcessed);
                } else
                {
                    _logger.LogInformation($"Doesn't look like we'd previously processed any of these. Going ahead with {messageBatch.Messages.Count} messages to find information on");
                }
            }
            return messageBatch;
        }
    }

    static class DateTimeExtensions
    {
        public static DateTime JumpAheadBy(this DateTime startDate, int numberOfJumps, PeriodType sizeOfJumps)
        {
            //calculate end date
            switch (sizeOfJumps)
            {
                case PeriodType.Year:
                    return startDate.AddYears(numberOfJumps);
                case PeriodType.Month:
                    return startDate.AddMonths(numberOfJumps);
            }
            return DateTime.MinValue;

        }
    }
}
