using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FastQuiz.Quiz.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddQuestionReviewState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "iscorrect",
                table: "questionrecords",
                type: "boolean",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "boolean");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "iscorrect",
                table: "questionrecords",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldNullable: true);
        }
    }
}
