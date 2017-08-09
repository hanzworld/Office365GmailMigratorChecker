using Newtonsoft.Json;
using Office365GmailMigratorChecker.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Office365GmailMigratorChecker
{
    class LocalPersistanceService
    {

        public static void PersistResultsToFile(MessageBatch messages, int year, int periods, PeriodType periodtype)
        {
            //sanity check we don't have
            if (messages.Messages.Count == 0)
            {
                throw new Exception("You're asking me to save an empty file Mr President, that's not supposed to happen");
            }

            using (StreamWriter file = File.CreateText(String.Format(@"Office365DataStore-{0}-{1}{2}.json", year, periods, periodtype)))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, messages);
            }
        }

        public static MessageBatch ReadResultsFromFile(int year, int periods, PeriodType periodtype)
        {
            using (FileStream stream = new FileStream(String.Format(@"Office365DataStore-{0}-{1}{2}.json", year, periods, periodtype), FileMode.Open))
            using (StreamReader file = new StreamReader(stream))
            {
                JsonSerializer serializer = new JsonSerializer();
                var messages = (MessageBatch)serializer.Deserialize(file, typeof(MessageBatch));
                Console.WriteLine("Read {0} objects from file", messages.Messages.Count);
                return messages;
            }

        }

        public static bool LocalFileExists(int year, int periods, PeriodType periodtype)
        {
            return File.Exists(String.Format(@"Office365DataStore-{0}-{1}{2}.json", year, periods, periodtype));
        }
    }
}
