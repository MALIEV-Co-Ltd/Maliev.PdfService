using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maliev.PdfService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplateCodeToRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TemplateCode",
                table: "GenerationRequests",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemplateCode",
                table: "GenerationRequests");
        }
    }
}
