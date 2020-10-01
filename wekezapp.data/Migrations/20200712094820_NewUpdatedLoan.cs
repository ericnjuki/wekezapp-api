using Microsoft.EntityFrameworkCore.Migrations;

namespace wekezapp.data.Migrations
{
    public partial class NewUpdatedLoan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Loan_AmountPaidSoFar",
                table: "Transaction");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Loan_AmountPaidSoFar",
                table: "Transaction",
                nullable: true);
        }
    }
}
