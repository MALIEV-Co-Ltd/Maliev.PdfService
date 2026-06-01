using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maliev.PdfService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGenerationRequestStoragePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StoragePath",
                table: "GenerationRequests",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoragePath",
                table: "GenerationRequests");
        }
    }
}
