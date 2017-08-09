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
            messages.ForEach(x => CreateRecordEntry(x));
        }

        private void CreateRecordEntry(MyMessage message)
        {
            using (var context = new MyMessageDbContext())
            {
                context.Messages.Add(message);
            }
          
        }
    }
}
