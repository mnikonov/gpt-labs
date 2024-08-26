using Microsoft.EntityFrameworkCore.Migrations;

namespace Gpt.Labs.Migrations
{
    /// <inheritdoc />
    public partial class OpenAIImageSettingsModelId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ModelId",
                table: "Settings",
                type: "TEXT",
                maxLength: 250,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 250,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ModelId",
                table: "Settings",
                type: "TEXT",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 250);
        }
    }
}
