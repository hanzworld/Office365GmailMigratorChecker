using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace Office365GmailMigratorChecker
{

    public class MyMessage
    {
        public Message OutlookMessage { get; set; }
        public string Rfc822MsgId { get; set; }
        public string GmailId { get; set; }
        public string Office365Id { get; set; }
        public bool IsMigratedToGmail { get; set; }
        public string Subject { get; set; }
        public DateTime SentDateTime { get; set; }
        public Message OutlookMessage { get; set; }
    }
}