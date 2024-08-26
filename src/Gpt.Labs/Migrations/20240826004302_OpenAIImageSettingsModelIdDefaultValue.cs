using Microsoft.EntityFrameworkCore.Migrations;

namespace Gpt.Labs.Migrations
{
    /// <inheritdoc />
    public partial class OpenAIImageSettingsModelIdDefaultValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"UPDATE Settings
                                     SET ModelId = 'dall-e-2'
                                   WHERE Type = 2 AND 
                                         ModelId = '';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
