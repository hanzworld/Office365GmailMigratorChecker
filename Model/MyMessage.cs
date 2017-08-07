using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace Office365GmailMigratorChecker
{

    public class MyMessage
    {
        public Message outlookMessage { get; set; }
        public bool isInGmail { get; set; }
        public string gmailId { get; set; }
    }
}
