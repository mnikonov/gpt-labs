using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gpt.Labs.Migrations
{
    /// <inheritdoc />
    public partial class OpenAISettingsHotKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HotKey",
                table: "Settings",
                type: "TEXT",
                maxLength: 25,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HotKey",
                table: "Settings");
        }
    }
}
