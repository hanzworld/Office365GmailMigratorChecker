using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Office365GmailMigratorChecker
{
    class LocalPersistanceService
    {

        public static void PersistResultsToFile(List<MyMessage> messages, int year, int periods, PeriodType periodtype)
        {
            using (StreamWriter file = File.CreateText(String.Format(@"Office365DataStore-{0}-{1}{2}.json", year, periods, periodtype)))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, messages);
            }
        }

        public static List<MyMessage> ReadResultsFromFile(int year, int periods, PeriodType periodtype)
        {
            using (FileStream stream = new FileStream(String.Format(@"Office365DataStore-{0}-{1}{2}.json", year, periods, periodtype), FileMode.Open))
            using (StreamReader file = new StreamReader(stream))
            {
                JsonSerializer serializer = new JsonSerializer();
                var messages = (List<MyMessage>)serializer.Deserialize(file, typeof(List<MyMessage>));
                Console.WriteLine("Read {0} objects from file", messages.Count);
                return messages;
            }

        }

        public static bool LocalFileExists(int year, int periods, PeriodType periodtype)
        {
            return File.Exists(String.Format(@"Office365DataStore-{0}-{1}{2}.json", year, periods, periodtype));
        }
    }
}
