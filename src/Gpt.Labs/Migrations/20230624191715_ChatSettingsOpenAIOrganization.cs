using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gpt.Labs.Migrations
{
    /// <inheritdoc />
    public partial class ChatSettingsOpenAIOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OpenAIOrganization",
                table: "Settings",
                type: "TEXT",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpenAIOrganization",
                table: "Settings");
        }
    }
}
