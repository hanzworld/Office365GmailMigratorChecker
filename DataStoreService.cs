using Google.Cloud.Datastore.V1;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Office365GmailMigratorChecker
{
    class DataStoreService
    {
        private KeyFactory keyFactory;
        private DatastoreDb db;

        public DataStoreService()
        {
            db = InstantiateDataStore();
            keyFactory = db.CreateKeyFactory("Message");
        }

        private DatastoreDb InstantiateDataStore()
        {
            DatastoreDb db = DatastoreDb.Create("barrowside-mail-maintainer");
            return db;
        }

        public void WriteToDb(List<MyMessage> messages)
        {
            messages.ForEach(x => CreateRecordEntry(x, keyFactory));
        }

        private void CreateRecordEntry(MyMessage message, KeyFactory keyFactory)
        {
            var messageToAdd = new Entity()
            {
                Key = keyFactory.CreateKey(message.outlookMessage.InternetMessageId),
                ["rfc822msgid"] = message.outlookMessage.InternetMessageId,
                ["gmailId"] = message.gmailId,
                ["office365Id"] = message.outlookMessage.Id,
                ["subject"] = new Google.Cloud.Datastore.V1.Value()
                {
                    StringValue = message.outlookMessage.InternetMessageId,
                    ExcludeFromIndexes = true
                },
                ["sentDateTime"] = new Google.Cloud.Datastore.V1.Value()
                {
                    TimestampValue = Timestamp.FromDateTime(message.outlookMessage.SentDateTime.Value.DateTime),
                    ExcludeFromIndexes = true,
                },
                ["migratedToGmail"] = message.outlookMessage.InternetMessageId,


            };
        }
    }
}
