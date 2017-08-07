using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace Office365GmailMigratorChecker
{

    public class MyMessage
    {
        public Message OutlookMessage { get; set; }
        public string GmailId { get; set; }
        public bool IsMigratedToGmail { get; set; }
    }
}