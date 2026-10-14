using FastQuiz.Quiz.Infrastructure.Repositories.PostgreSQL;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FastQuiz.Quiz.Migrations.PostgreSql;

[DbContext(typeof(PostgreSQLQuizDbContext))]
[Migration("20261014020001_ResetPendingQuestionReviewState")]
public class ResetPendingQuestionReviewState : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE questionrecords
            SET iscorrect = NULL
            WHERE quizattemptid IN (SELECT id FROM quizattempts WHERE status <> 3);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Irreversible data repair; completed attempts have not been modified.
    }
}
