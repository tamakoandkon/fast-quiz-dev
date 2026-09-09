using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FastQuiz.Quiz.Migrations
{
    /// <inheritdoc />
    public partial class InitialQuizDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuizTemplates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(
                        type: "TEXT",
                        maxLength: 10000,
                        nullable: false
                    ),
                    Remark = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    OwnerId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    AttemptLimit = table.Column<int>(type: "INTEGER", nullable: false),
                    IsUnlimitedTime = table.Column<bool>(type: "INTEGER", nullable: false),
                    BeginTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    DurationMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPublished = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizTemplates", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "QuestionSelectionRules",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    QuizeTemplateId = table.Column<string>(
                        type: "TEXT",
                        maxLength: 128,
                        nullable: false
                    ),
                    GroupId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Count = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionSelectionRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionSelectionRules_QuizTemplates_QuizeTemplateId",
                        column: x => x.QuizeTemplateId,
                        principalTable: "QuizTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "QuestionSnapshots",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    QuizTemplateId = table.Column<string>(
                        type: "TEXT",
                        maxLength: 128,
                        nullable: false
                    ),
                    QuestionId = table.Column<string>(
                        type: "TEXT",
                        maxLength: 128,
                        nullable: false
                    ),
                    MarkUpContent = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    GroupId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ParentId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionSnapshots_QuizTemplates_QuizTemplateId",
                        column: x => x.QuizTemplateId,
                        principalTable: "QuizTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "QuizSnapshots",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    QuizTemplateId = table.Column<string>(
                        type: "TEXT",
                        maxLength: 128,
                        nullable: false
                    ),
                    TotalScore = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizSnapshots_QuizTemplates_QuizTemplateId",
                        column: x => x.QuizTemplateId,
                        principalTable: "QuizTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "QuestionAnswerSnapshots",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    MarkUpContent = table.Column<string>(type: "TEXT", nullable: false),
                    IsCorrect = table.Column<bool>(type: "INTEGER", nullable: false),
                    QuestionSnapshotId = table.Column<string>(
                        type: "TEXT",
                        maxLength: 128,
                        nullable: false
                    ),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Strategy = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionAnswerSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionAnswerSnapshots_QuestionSnapshots_QuestionSnapshotId",
                        column: x => x.QuestionSnapshotId,
                        principalTable: "QuestionSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "QuizAttempts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    AssignId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    QuizTemplateId = table.Column<string>(
                        type: "TEXT",
                        maxLength: 128,
                        nullable: false
                    ),
                    QuizSnapshotId = table.Column<string>(
                        type: "TEXT",
                        maxLength: 128,
                        nullable: false
                    ),
                    StartTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    SubmitTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_QuizSnapshots_QuizSnapshotId",
                        column: x => x.QuizSnapshotId,
                        principalTable: "QuizSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_QuizAttempts_QuizTemplates_QuizTemplateId",
                        column: x => x.QuizTemplateId,
                        principalTable: "QuizTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "QuizSnapshotQuestionSnapshotMaps",
                columns: table => new
                {
                    QuizSnapshotId = table.Column<string>(
                        type: "TEXT",
                        maxLength: 128,
                        nullable: false
                    ),
                    QuestionSnapshotId = table.Column<string>(
                        type: "TEXT",
                        maxLength: 128,
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_QuizSnapshotQuestionSnapshotMaps",
                        x => new { x.QuizSnapshotId, x.QuestionSnapshotId }
                    );
                    table.ForeignKey(
                        name: "FK_QuizSnapshotQuestionSnapshotMaps_QuestionSnapshots_QuestionSnapshotId",
                        column: x => x.QuestionSnapshotId,
                        principalTable: "QuestionSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_QuizSnapshotQuestionSnapshotMaps_QuizSnapshots_QuizSnapshotId",
                        column: x => x.QuizSnapshotId,
                        principalTable: "QuizSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "QuestionRecords",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    MarkUpContent = table.Column<string>(type: "TEXT", nullable: false),
                    IsCorrect = table.Column<bool>(type: "INTEGER", nullable: false),
                    QuizAttemptId = table.Column<string>(
                        type: "TEXT",
                        maxLength: 128,
                        nullable: false
                    ),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestionSnapshotId = table.Column<string>(
                        type: "TEXT",
                        maxLength: 128,
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionRecords_QuestionSnapshots_QuestionSnapshotId",
                        column: x => x.QuestionSnapshotId,
                        principalTable: "QuestionSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_QuestionRecords_QuizAttempts_QuizAttemptId",
                        column: x => x.QuizAttemptId,
                        principalTable: "QuizAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "AnswerRecords",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    QuizAttemptId = table.Column<string>(
                        type: "TEXT",
                        maxLength: 128,
                        nullable: false
                    ),
                    QuestionRecordId = table.Column<string>(
                        type: "TEXT",
                        maxLength: 128,
                        nullable: false
                    ),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnswerRecords_QuestionRecords_QuestionRecordId",
                        column: x => x.QuestionRecordId,
                        principalTable: "QuestionRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_AnswerRecords_QuizAttempts_QuizAttemptId",
                        column: x => x.QuizAttemptId,
                        principalTable: "QuizAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_AnswerRecords_QuestionRecordId",
                table: "AnswerRecords",
                column: "QuestionRecordId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_AnswerRecords_QuizAttemptId",
                table: "AnswerRecords",
                column: "QuizAttemptId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_QuestionAnswerSnapshots_QuestionSnapshotId_SortOrder",
                table: "QuestionAnswerSnapshots",
                columns: new[] { "QuestionSnapshotId", "SortOrder" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_QuestionRecords_QuestionSnapshotId",
                table: "QuestionRecords",
                column: "QuestionSnapshotId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_QuestionRecords_QuizAttemptId_SortOrder",
                table: "QuestionRecords",
                columns: new[] { "QuizAttemptId", "SortOrder" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSelectionRules_QuizeTemplateId",
                table: "QuestionSelectionRules",
                column: "QuizeTemplateId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSnapshots_GroupId_Type_Level",
                table: "QuestionSnapshots",
                columns: new[] { "GroupId", "Type", "Level" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSnapshots_QuestionId",
                table: "QuestionSnapshots",
                column: "QuestionId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSnapshots_QuizTemplateId",
                table: "QuestionSnapshots",
                column: "QuizTemplateId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_AssignId",
                table: "QuizAttempts",
                column: "AssignId",
                filter: "AssignId IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_QuizSnapshotId",
                table: "QuizAttempts",
                column: "QuizSnapshotId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_QuizTemplateId_Status",
                table: "QuizAttempts",
                columns: new[] { "QuizTemplateId", "Status" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_QuizSnapshotQuestionSnapshotMaps_QuestionSnapshotId",
                table: "QuizSnapshotQuestionSnapshotMaps",
                column: "QuestionSnapshotId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_QuizSnapshots_QuizTemplateId",
                table: "QuizSnapshots",
                column: "QuizTemplateId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_QuizTemplates_OwnerId_IsPublished",
                table: "QuizTemplates",
                columns: new[] { "OwnerId", "IsPublished" }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AnswerRecords");

            migrationBuilder.DropTable(name: "QuestionAnswerSnapshots");

            migrationBuilder.DropTable(name: "QuestionSelectionRules");

            migrationBuilder.DropTable(name: "QuizSnapshotQuestionSnapshotMaps");

            migrationBuilder.DropTable(name: "QuestionRecords");

            migrationBuilder.DropTable(name: "QuestionSnapshots");

            migrationBuilder.DropTable(name: "QuizAttempts");

            migrationBuilder.DropTable(name: "QuizSnapshots");

            migrationBuilder.DropTable(name: "QuizTemplates");
        }
    }
}
