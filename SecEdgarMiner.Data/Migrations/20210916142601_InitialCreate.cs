using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SecEdgarMiner.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Form4Info",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodOfReport = table.Column<DateTime>(nullable: false),
                    OwnerName = table.Column<string>(maxLength: 100, nullable: false),
                    OwnerCik = table.Column<string>(maxLength: 100, nullable: false),
                    OwnerOfficerTitle = table.Column<string>(maxLength: 100, nullable: true),
                    OwnerIsOfficer = table.Column<bool>(nullable: false),
                    IssuerName = table.Column<string>(maxLength: 100, nullable: false),
                    IssuerTradingSymbol = table.Column<string>(maxLength: 20, nullable: false),
                    IssuerCik = table.Column<string>(maxLength: 100, nullable: false),
                    HtmlUrl = table.Column<string>(maxLength: 500, nullable: false),
                    XmlUrl = table.Column<string>(maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Form4Info", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Form4Info_HtmlUrl",
                table: "Form4Info",
                column: "HtmlUrl",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Form4Info");
        }
    }
}
