using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Office365GmailMigratorChecker
{
    class SqlExpressService
    {

        public void WriteToDb(List<MyMessage> messages)
        {
            using (var context = new MyMessageDbContext())
            {
                context.Messages.AddRange(messages);
                context.SaveChanges();
            }
        }

        private void CreateRecordEntry(MyMessage message)
        {
            using (var context = new MyMessageDbContext())
            {
                context.Messages.Add(message);
                context.SaveChanges();
            }
          
        }
    }
}
