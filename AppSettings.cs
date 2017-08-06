using System;
using System.Collections.Generic;
using System.Text;

namespace Office365GmailMigratorChecker
{
    public class AppSettings
    {
            public int Periods { get; set; }
            public string PeriodType { get; set; }
            public int StartYear { get; set; }
    }

    public class Gmail
    {
        public Gmail() { }
        public string Username { get; set; }
        public string CredentialPath { get; set; }
    }

    public class Graph
    {
        public Graph() { }

        public string Username { get; set; } = "default";
        public string Tenant { get; set; }
        public string ClientId { get; set; }
        public string Secret { get; set; }
        public int BatchSize { get; set; } = 100;
    }
}
