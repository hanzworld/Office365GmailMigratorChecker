using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Office365GmailMigratorChecker
{
    class LocalPersistanceService
    {

        public static void PersistResultsToFile(List<MyMessage> messages)
        {
            using (StreamWriter file = System.IO.File.CreateText(@"Office365DataStore.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, messages);
            }
        }

        public static List<MyMessage> ReadResultsFromFile()
        {
            using (FileStream stream = new FileStream(@"Office365DataStore.json", FileMode.Open))
            using (StreamReader file = new StreamReader(stream))
            {
                JsonSerializer serializer = new JsonSerializer();
                var messages = (List<MyMessage>)serializer.Deserialize(file, typeof(List<MyMessage>));
                Console.WriteLine("Read {0} objects from file", messages.Count);
                return messages;
            }

        }
    }
}
