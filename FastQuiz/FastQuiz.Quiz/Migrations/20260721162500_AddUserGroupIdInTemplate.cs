using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FastQuiz.Quiz.Migrations
{
    /// <inheritdoc />
    public partial class AddUserGroupIdInTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserGroupId",
                table: "QuizTemplates",
                type: "TEXT",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserGroupId",
                table: "QuizTemplates");
        }
    }
}
