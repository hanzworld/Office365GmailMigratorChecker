using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Office365GmailMigratorChecker.Migrations
{
    public partial class FirstMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER DATABASE [Office365GmailMigratorChecker] COLLATE SQL_Latin1_General_CP1_CS_AS;", suppressTransaction: true);

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Rfc822MsgId = table.Column<string>(nullable: false),
                    GmailId = table.Column<string>(type: "VARCHAR(255)", nullable: true),
                    IsMigratedToGmail = table.Column<bool>(nullable: false),
                    Office365Id = table.Column<string>(type: "VARCHAR(255)", nullable: false),
                    SentDateTime = table.Column<DateTime>(nullable: false),
                    Subject = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Rfc822MsgId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_GmailId",
                table: "Messages",
                column: "GmailId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Office365Id",
                table: "Messages",
                column: "Office365Id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");
        }
    }
}
