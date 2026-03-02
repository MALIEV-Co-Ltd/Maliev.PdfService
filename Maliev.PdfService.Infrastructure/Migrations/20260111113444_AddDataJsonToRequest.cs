using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maliev.PdfService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDataJsonToRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataJson",
                table: "GenerationRequests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataJson",
                table: "GenerationRequests");
        }
    }
}
