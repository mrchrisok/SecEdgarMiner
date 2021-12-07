using Microsoft.EntityFrameworkCore.Migrations;

namespace SecEdgarMiner.Data.Migrations
{
    public partial class Add_TransactionTotal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PurchaseTransactionsTotal",
                table: "Form4Info",
                type: "decimal(18,2)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PurchaseTransactionsTotal",
                table: "Form4Info");
        }
    }
}
