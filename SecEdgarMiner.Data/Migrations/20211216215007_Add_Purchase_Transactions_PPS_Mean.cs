using Microsoft.EntityFrameworkCore.Migrations;

namespace SecEdgarMiner.Data.Migrations
{
    public partial class Add_Purchase_Transactions_PPS_Mean : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PurchaseTransactionsPricePerSecurityMean",
                table: "Form4Info",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PurchaseTransactionsPricePerSecurityMean",
                table: "Form4Info");
        }
    }
}
