using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FastQuiz.Question.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PostgreSqlInitialDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "questiongroups",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    remark = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ownerid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questiongroups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "questions",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    markupcontent = table.Column<string>(type: "text", nullable: false),
                    remark = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    groupid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questions", x => x.id);
                    table.ForeignKey(
                        name: "FK_questions_questiongroups_groupid",
                        column: x => x.groupid,
                        principalTable: "questiongroups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "questionanswers",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    questionid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    markupcontent = table.Column<string>(type: "text", nullable: false),
                    iscorrect = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    sortorder = table.Column<int>(type: "integer", nullable: false),
                    MatchStrategy = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questionanswers", x => x.id);
                    table.ForeignKey(
                        name: "FK_questionanswers_questions_questionid",
                        column: x => x.questionid,
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionAnswers_QuestionId_SortOrder",
                table: "questionanswers",
                columns: new[] { "questionid", "sortorder" });

            migrationBuilder.CreateIndex(
                name: "IX_questiongroups_ownerid_name",
                table: "questiongroups",
                columns: new[] { "ownerid", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_GroupId_Type_Level",
                table: "questions",
                columns: new[] { "groupid", "type", "level" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "questionanswers");

            migrationBuilder.DropTable(
                name: "questions");

            migrationBuilder.DropTable(
                name: "questiongroups");
        }
    }
}
