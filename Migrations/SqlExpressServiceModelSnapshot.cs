using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Office365GmailMigratorChecker;

namespace Office365GmailMigratorChecker.Migrations
{
    [DbContext(typeof(MyMessageDbContext))]
    partial class SqlExpressServiceModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Office365GmailMigratorChecker.MyMessage", b =>
                {
                    b.Property<string>("Rfc822MsgId");

                    b.Property<string>("GmailId")
                        .HasColumnType("VARCHAR(255)");

                    b.Property<bool>("IsMigratedToGmail");

                    b.Property<string>("Office365Id")
                        .IsRequired()
                        .HasColumnType("VARCHAR(255)");

                    b.Property<DateTime>("SentDateTime");

                    b.Property<string>("Subject")
                        .IsRequired();

                    b.HasKey("Rfc822MsgId");

                    b.HasIndex("GmailId")
                        .IsUnique();

                    b.HasIndex("Office365Id")
                        .IsUnique();

                    b.ToTable("Messages");
                });
        }
    }
}
