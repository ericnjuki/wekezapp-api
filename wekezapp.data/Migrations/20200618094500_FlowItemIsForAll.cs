using Microsoft.EntityFrameworkCore.Migrations;

namespace wekezapp.data.Migrations
{
    public partial class FlowItemIsForAll : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsForAll",
                table: "FlowItems",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsForAll",
                table: "FlowItems");
        }
    }
}
