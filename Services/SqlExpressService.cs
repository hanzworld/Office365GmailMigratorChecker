using Microsoft.Extensions.Logging;
using Office365GmailMigratorChecker.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Office365GmailMigratorChecker
{
    class SqlExpressService
    {
        private ILogger<SqlExpressService> _logger;

        public SqlExpressService(ILogger<SqlExpressService> logger)
        {
            _logger = logger;
        }

        public void WriteToDb(List<MyMessage> messages)
        {
            using (var context = new MyMessageDbContext())
            {
                foreach(var message in messages)
                {
                    context.Messages.AddOrUpdate(message);
                }
                    
                context.SaveChanges();
                _logger.LogInformation($"Saved {context.ChangeTracker.Entries().Count()} of {messages.Count} records to database ({context.ChangeTracker.Entries().Count(e => e.State == Microsoft.EntityFrameworkCore.EntityState.Modified)} updates, {context.ChangeTracker.Entries().Count(e => e.State == Microsoft.EntityFrameworkCore.EntityState.Added)} inserts, {context.ChangeTracker.Entries().Count(e => e.State == Microsoft.EntityFrameworkCore.EntityState.Unchanged)} existing)");
                    
            }
        }

        private void WriteToDb(MyMessage message)
        {
            using (var context = new MyMessageDbContext())
            {
                context.Messages.Add(message);
                context.SaveChanges();
            }
          
        }
    }
}
