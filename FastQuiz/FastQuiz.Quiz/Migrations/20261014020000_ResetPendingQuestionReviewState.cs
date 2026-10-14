using FastQuiz.Quiz.Infrastructure.Repositories.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FastQuiz.Quiz.Migrations;

[DbContext(typeof(SqliteQuizDbContext))]
[Migration("20261014020000_ResetPendingQuestionReviewState")]
public class ResetPendingQuestionReviewState : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Legacy false is ambiguous. Recheck unfinished attempts, retaining all final results.
        migrationBuilder.Sql("""
            UPDATE "QuestionRecords"
            SET "IsCorrect" = NULL
            WHERE "QuizAttemptId" IN (SELECT "Id" FROM "QuizAttempts" WHERE "Status" <> 3);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // The original review decisions cannot be reconstructed from the legacy boolean.
    }
}
