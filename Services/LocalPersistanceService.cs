using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Office365GmailMigratorChecker.Model;
using System;
using System.IO;

namespace Office365GmailMigratorChecker
{
    class LocalPersistanceService
    {
        private ILogger<LocalPersistanceService> _logger;

        public LocalPersistanceService(ILogger<LocalPersistanceService> logger)
        {
            _logger = logger;
        }

        public static void PersistResultsToFile(MessageBatch batch)
        {
            //sanity check we don't have
            if (batch.Messages == null || batch.Messages.Count == 0)
            {
                throw new Exception("You're asking me to save an empty file Mr President, that's not supposed to happen");
            }

            using (StreamWriter file = File.CreateText(ConstructFileName(batch)))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, batch);
            }
        }

        public static MessageBatch ReadResultsFromFile(MessageBatch batch)
        {
            try
            {
                using (FileStream stream = new FileStream(ConstructFileName(batch), FileMode.Open))
                using (StreamReader file = new StreamReader(stream))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    var messages = (MessageBatch)serializer.Deserialize(file, typeof(MessageBatch));
                    Console.WriteLine("Read {0} objects from file", messages.Messages.Count);
                    return messages;
                }
            }
            catch (Exception e)
            {
               //TODO handle exceptions
            }
            return null;

        }

        public static bool LocalFileExists(MessageBatch batch)
        {
            return File.Exists(ConstructFileName(batch));
        }
        
        private static string ConstructFileName(MessageBatch batch)
        {
            return String.Format(@"Office365DataStore-{0}-{1}.json", batch.StartDate, batch.EndDate);
        }

    }
}
