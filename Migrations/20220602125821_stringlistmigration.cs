using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzureTest.Migrations
{
    public partial class stringlistmigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Primitive");

            migrationBuilder.AddColumn<string>(
                name: "Attributes",
                table: "Emails",
                type: "json",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attributes",
                table: "Emails");

            migrationBuilder.CreateTable(
                name: "Primitive",
                columns: table => new
                {
                    PrimitiveId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmailClassKey = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Data = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Primitive", x => x.PrimitiveId);
                    table.ForeignKey(
                        name: "FK_Primitive_Emails_EmailClassKey",
                        column: x => x.EmailClassKey,
                        principalTable: "Emails",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Primitive_EmailClassKey",
                table: "Primitive",
                column: "EmailClassKey");
        }
    }
}
