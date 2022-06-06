using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzureTest.Migrations
{
    public partial class totalatrsmigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "attributesReceivedToday",
                table: "SendEmails",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "attributesReceivedToday",
                table: "SendEmails");
        }
    }
}
