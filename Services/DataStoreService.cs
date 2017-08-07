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
                Key = keyFactory.CreateKey(message.OutlookMessage.InternetMessageId),
                ["rfc822msgid"] = message.OutlookMessage.InternetMessageId,
                ["gmailId"] = message.GmailId,
                ["office365Id"] = message.OutlookMessage.Id,
                ["subject"] = new Google.Cloud.Datastore.V1.Value()
                {
                    StringValue = message.OutlookMessage.InternetMessageId,
                    ExcludeFromIndexes = true
                },
                ["sentDateTime"] = new Google.Cloud.Datastore.V1.Value()
                {
                    TimestampValue = Timestamp.FromDateTime(message.OutlookMessage.SentDateTime.Value.DateTime),
                    ExcludeFromIndexes = true,
                },
                ["migratedToGmail"] = message.OutlookMessage.InternetMessageId,


            };
        }
    }
}
