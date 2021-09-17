using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SecEdgarMiner.Data.Migrations
{
    public partial class AddAuditDates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "Form4Info",
                nullable: false,
                defaultValueSql: "getutcdate()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateModified",
                table: "Form4Info",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "Form4Info");

            migrationBuilder.DropColumn(
                name: "DateModified",
                table: "Form4Info");
        }
    }
}
