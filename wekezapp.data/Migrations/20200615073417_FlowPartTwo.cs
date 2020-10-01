using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace wekezapp.data.Migrations
{
    public partial class FlowPartTwo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateModified",
                table: "FlowItems",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "NotificationType",
                table: "FlowItems",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "Amount",
                table: "Documents",
                nullable: false,
                defaultValue: 0f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateModified",
                table: "FlowItems");

            migrationBuilder.DropColumn(
                name: "NotificationType",
                table: "FlowItems");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Documents");
        }
    }
}
