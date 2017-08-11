using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Office365GmailMigratorChecker.Model;
using System;
using System.Collections.Generic;
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

        public void PersistResultsToFile(MessageBatch batch)
        {
            //sanity check we don't have
            if (batch.Messages == null || batch.Messages.Count == 0)
            {
                throw new Exception("You're asking me to save an empty file Mr President, that's not supposed to happen");
            }

            using (StreamWriter file = File.CreateText(ConstructFileName(batch)))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, batch.Messages);
            }
        }

        public List<MyMessage> ReadResultsFromFile(MessageBatch batch)
        {
            try
            {
                //TODO output more useful info liek the filename
                using (FileStream stream = new FileStream(ConstructFileName(batch), FileMode.Open))
                using (StreamReader file = new StreamReader(stream))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    ITraceWriter writer = new MemoryTraceWriter();
                    serializer.TraceWriter = writer;
                    var messages = (List<MyMessage>)serializer.Deserialize(file, typeof(List<MyMessage>));
                    _logger.LogDebug("Read {0} objects from file", messages.Count);
                    return messages;
                }
            }
            catch (Exception e)
            {
               //TODO handle exceptions
            }
            return null;

        }

        public bool LocalFileExists(MessageBatch batch)
        {
            return File.Exists(ConstructFileName(batch));
        }
        
        private string ConstructFileName(MessageBatch batch)
        {
            return String.Format(@"Office365DataStore-{0}-{1}.json", batch.StartDate.ToString("yyyy-MM-dd"), batch.EndDate.ToString("yyyy-MM-dd"));
        }

    }
}
