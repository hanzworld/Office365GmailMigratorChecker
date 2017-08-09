using Microsoft.EntityFrameworkCore;
using Office365GmailMigratorChecker.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Office365GmailMigratorChecker
{
    class MyMessageDbContext : DbContext
    {
        public DbSet<MyMessage> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Office365GmailMigratorChecker;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MyMessage>().HasIndex(b => b.Office365Id).IsUnique();
            modelBuilder.Entity<MyMessage>().HasIndex(b => b.GmailId).IsUnique();
        }
        
    }
}
