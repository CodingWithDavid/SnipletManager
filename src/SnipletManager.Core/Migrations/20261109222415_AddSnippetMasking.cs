using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SnipletManager.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddSnippetMasking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMasked",
                table: "Snippets",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMasked",
                table: "Snippets");
        }
    }
}
