﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Office365GmailMigratorChecker.Model
{
    class MessageBatch
    {
        [JsonIgnore]
        private int startYear;
        [JsonIgnore]
        private int period;
        [JsonIgnore]
        private PeriodType periodLength;

        public MessageBatch()
        {
        }        

        public MessageBatch(int startYear, int period, PeriodType periodLength)
        {
            this.periodLength = periodLength;
        }

        
        public List<MyMessage> Messages { get; set; }

        [JsonIgnore]
        public List<MyMessage> ConfirmedMigrationStatus { get {
                return Messages.Where(m => m.IsMigratedToGmail.HasValue).ToList();
            } }

        [JsonIgnore]
        public List<MyMessage> UnconfirmedMigrationStatus { get {
               return Messages.Where(m => !m.IsMigratedToGmail.HasValue).ToList();
            } }

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


            return String.Format("Totals: {1} already matched/unmatched, {2} found to match (will include previously matched), {3} confirmed unmatched, {4} unknown)",
                    numberAlreadyProcessed,
                    countOfMessagesMigrated,
                    countOfMessagesNotMigrated,
                    countOfMessagesUnknownIfMigrated);
        }

    }
}
