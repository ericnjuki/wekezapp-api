using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace wekezapp.data.Migrations
{
    public partial class UhmIStartOver : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chamas",
                columns: table => new
                {
                    ChamaId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ChamaName = table.Column<string>(nullable: false),
                    Balance = table.Column<float>(nullable: false),
                    LoanInterestRate = table.Column<float>(nullable: false),
                    LatePaymentFineRate = table.Column<float>(nullable: false),
                    MinimumContribution = table.Column<float>(nullable: false),
                    Period = table.Column<int>(nullable: false),
                    MgrOrder = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chamas", x => x.ChamaId);
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    TransactionId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Amount = table.Column<float>(nullable: false),
                    IsClosed = table.Column<bool>(nullable: false),
                    DateClosed = table.Column<DateTime>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    DepositorId = table.Column<int>(nullable: true),
                    DateRequested = table.Column<DateTime>(nullable: true),
                    WithdrawerId = table.Column<int>(nullable: true),
                    ContributorId = table.Column<int>(nullable: true),
                    DateDue = table.Column<DateTime>(nullable: true),
                    ContributionId = table.Column<int>(nullable: true),
                    Rate = table.Column<float>(nullable: true),
                    DateApplied = table.Column<DateTime>(nullable: true),
                    ReceiverId = table.Column<int>(nullable: true),
                    Approved = table.Column<bool>(nullable: true),
                    EvaluatedBy = table.Column<int>(nullable: true),
                    DateIssued = table.Column<DateTime>(nullable: true),
                    InterestRate = table.Column<float>(nullable: true),
                    AmountPayable = table.Column<float>(nullable: true),
                    AmountPaidSoFar = table.Column<float>(nullable: true),
                    LatePaymentFine = table.Column<float>(nullable: true),
                    IsDefaulted = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.TransactionId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FirstName = table.Column<string>(nullable: true),
                    SecondName = table.Column<string>(nullable: true),
                    Role = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    PasswordHash = table.Column<byte[]>(nullable: false),
                    PasswordSalt = table.Column<byte[]>(nullable: false),
                    Token = table.Column<string>(nullable: true),
                    Balance = table.Column<double>(nullable: false),
                    Stake = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    DocumentId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TransactionId = table.Column<int>(nullable: false),
                    DocumentType = table.Column<int>(nullable: false),
                    IsReversal = table.Column<bool>(nullable: false),
                    DebitTo = table.Column<int>(nullable: false),
                    CreditFrom = table.Column<int>(nullable: false),
                    TransactionDate = table.Column<DateTime>(nullable: false),
                    ConfirmedBy = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.DocumentId);
                    table.ForeignKey(
                        name: "FK_Documents_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_TransactionId",
                table: "Documents",
                column: "TransactionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chamas");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Transaction");
        }
    }
}
