using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace wekezapp.data.Migrations
{
    public partial class ChamaMGRDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "MgrAmount",
                table: "Chamas",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextMgrDate",
                table: "Chamas",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "NextMgrReceiverIndex",
                table: "Chamas",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MgrAmount",
                table: "Chamas");

            migrationBuilder.DropColumn(
                name: "NextMgrDate",
                table: "Chamas");

            migrationBuilder.DropColumn(
                name: "NextMgrReceiverIndex",
                table: "Chamas");
        }
    }
}
