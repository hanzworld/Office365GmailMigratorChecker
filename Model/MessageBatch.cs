using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Office365GmailMigratorChecker.Model
{
    class MessageBatch
    {
        private LocalPersistanceService _localPersistanceService;

        public MessageBatch(LocalPersistanceService localPersistanceService, DateTime startDate, DateTime endDate)
        {
            EndDate = endDate;
            StartDate = startDate;
            _localPersistanceService = localPersistanceService;
            //if we've already got a local copy, load us up straight away
            LoadFromLocalCacheIfAvailable();
        }

        public List<MyMessage> Messages { get; set; }
        [JsonProperty]
        public DateTime StartDate { get; private set; }
        [JsonProperty]
        public DateTime EndDate { get; private set; }
        [JsonIgnore]
        public bool RetrievedFromCache { get; private set; }

        #region Helper Linq Methods
        [JsonIgnore]
        public List<MyMessage> ConfirmedMigrationStatus
        {
            get
            {
                return Messages.Where(m => m.IsMigratedToGmail.HasValue).ToList();
            }
        }

        [JsonIgnore]
        public List<MyMessage> UnconfirmedMigrationStatus
        {
            get
            {
                return Messages.Where(m => !m.IsMigratedToGmail.HasValue).ToList();
            }
        }

        [JsonIgnore]
        public List<MyMessage> MigratedMessages
        {
            get
            {
                return Messages.Where(m => m.IsMigratedToGmail.GetValueOrDefault()).ToList();
            }
        }

        [JsonIgnore]
        public List<MyMessage> NotMigratedMessages
        {
            get
            {
                return Messages.Where(m => m.IsMigratedToGmail.HasValue && !m.IsMigratedToGmail.Value).ToList();
            }
        }

        #endregion

        private void LoadFromLocalCacheIfAvailable()
        {
            if (_localPersistanceService.LocalFileExists(this))
            {
                this.Messages = _localPersistanceService.ReadResultsFromFile(this).Messages;
                this.RetrievedFromCache = true;
            }
            else
            {
                this.RetrievedFromCache = false;
            }

        }

        public void Save()
        {
            _localPersistanceService.PersistResultsToFile(this);

        }

        public void Finish()
        {
            Save();
            //TODO - get the MessageBatch to print results of the import - it knows them!
        }


        //TODO: Make a better way to cater for reading from file 100 messages, 80 of which are already processed, 20 of which aren't, 
        //10 of which we subseuqntly process - need to make this far easier (and accurately accurate, which it isn't at the moment)

        public override string ToString()
        {
            return ToString(0);
        }

        string ToString(int numberAlreadyProcessed)
        {
            int countOfMessagesInBatch = Messages.Count;

            //remember messages have three states - either we know it was migrated, we know it wasn't migrated, or we have no idea at all
            int countOfMessagesWeKnowWhetherMigratedOrNot = ConfirmedMigrationStatus.Count - numberAlreadyProcessed;
            int countOfMessagesMigrated = MigratedMessages.Count;

            //therefore
            int countOfMessagesNotMigrated = countOfMessagesWeKnowWhetherMigratedOrNot - countOfMessagesMigrated;
            int countOfMessagesUnknownIfMigrated = countOfMessagesInBatch - countOfMessagesWeKnowWhetherMigratedOrNot;


            return String.Format("Totals: {0} already matched/unmatched, {1} found to match (will include previously matched), {2} confirmed unmatched, {3} unknown)",
                    numberAlreadyProcessed,
                    countOfMessagesMigrated,
                    countOfMessagesNotMigrated,
                    countOfMessagesUnknownIfMigrated);
        }

    }

    class MessageBatchFactory
    {
        private LocalPersistanceService _localPersistanceService;

        public MessageBatchFactory(LocalPersistanceService localPersistanceService)
        {
            _localPersistanceService = localPersistanceService;
        }

        public MessageBatch SetupBatch(DateTime startDate, DateTime endDate)
        {
            return new MessageBatch(_localPersistanceService, startDate, endDate);
        }
    }
}
