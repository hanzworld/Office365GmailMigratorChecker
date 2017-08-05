using Google.Cloud.Datastore.V1;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Office365GmailMigratorChecker
{
    class DataStoreService
    {

        public static DatastoreDb InstantiateDataStore()
        {
            DatastoreDb db = DatastoreDb.Create("barrowside-mail-maintainer");
            return db;
        }

        public static void WriteToDb(DatastoreDb db, KeyFactory keyFactory, MyMessage message)
        {
            var messageToAdd = new Google.Cloud.Datastore.V1.Entity()
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
