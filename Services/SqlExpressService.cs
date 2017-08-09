using Microsoft.EntityFrameworkCore;
using Office365GmailMigratorChecker.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Office365GmailMigratorChecker
{
    class SqlExpressService
    {

        public void WriteToDb(List<MyMessage> messages)
        {
            try
            {
                using (var context = new MyMessageDbContext())
                {
                    foreach(var message in messages)
                    {
                        context.Messages.AddOrUpdate(message);
                    }
                    context.SaveChanges();

                }
            }
            catch (Exception e)
            {
                //handle exception
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
