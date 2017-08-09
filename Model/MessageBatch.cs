using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Office365GmailMigratorChecker.Model
{
    class MessageBatch
    {
        public MessageBatch()
        {
        }        

        public List<MyMessage> Messages { get; set; }

        public List<MyMessage> ConfirmedMigrationStatus { get {
                return Messages.Where(m => m.IsMigratedToGmail.HasValue).ToList();
            } }

        public List<MyMessage> UnconfirmedMigrationStatus { get {
               return Messages.Where(m => !m.IsMigratedToGmail.HasValue).ToList();
            } }

        public List<MyMessage> MigratedMessages
        {
            get
            {
                return Messages.Where(m => m.IsMigratedToGmail.GetValueOrDefault()).ToList();
            }
        }

        public List<MyMessage> NotMigratedMessages
        {
            get
            {
                return Messages.Where(m => m.IsMigratedToGmail.HasValue && !m.IsMigratedToGmail.Value).ToList();
            }
        }
    }
}
