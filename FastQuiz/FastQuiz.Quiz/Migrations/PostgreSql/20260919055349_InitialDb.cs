using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FastQuiz.Quiz.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class InitialDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "quiztemplates",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ownerid = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    attemptlimit = table.Column<int>(type: "integer", nullable: false),
                    isunlimitedtime = table.Column<bool>(type: "boolean", nullable: false),
                    begintime = table.Column<string>(type: "text", nullable: false),
                    endtime = table.Column<string>(type: "text", nullable: false),
                    durationminutes = table.Column<int>(type: "integer", nullable: false),
                    ispublished = table.Column<bool>(type: "boolean", nullable: false),
                    usergroupid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    createdat = table.Column<string>(type: "text", nullable: false),
                    updatedat = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiztemplates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "questionselectionrules",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    quizetemplateid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    groupid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questionselectionrules", x => x.id);
                    table.ForeignKey(
                        name: "FK_questionselectionrules_quiztemplates_quizetemplateid",
                        column: x => x.quizetemplateid,
                        principalTable: "quiztemplates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "questionsnapshots",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    quiztemplateid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    questionid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    markupcontent = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    groupid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    parentid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questionsnapshots", x => x.id);
                    table.ForeignKey(
                        name: "FK_questionsnapshots_quiztemplates_quiztemplateid",
                        column: x => x.quiztemplateid,
                        principalTable: "quiztemplates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quizsnapshots",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    quiztemplateid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    totalscore = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quizsnapshots", x => x.id);
                    table.ForeignKey(
                        name: "FK_quizsnapshots_quiztemplates_quiztemplateid",
                        column: x => x.quiztemplateid,
                        principalTable: "quiztemplates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "questionanswersnapshots",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    markupcontent = table.Column<string>(type: "text", nullable: false),
                    iscorrect = table.Column<bool>(type: "boolean", nullable: false),
                    questionsnapshotid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    sortorder = table.Column<int>(type: "integer", nullable: false),
                    strategy = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questionanswersnapshots", x => x.id);
                    table.ForeignKey(
                        name: "FK_questionanswersnapshots_questionsnapshots_questionsnapshotid",
                        column: x => x.questionsnapshotid,
                        principalTable: "questionsnapshots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quizattempts",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    assignid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    quiztemplateid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    quizsnapshotid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    starttime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    submittime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    score = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quizattempts", x => x.id);
                    table.ForeignKey(
                        name: "FK_quizattempts_quizsnapshots_quizsnapshotid",
                        column: x => x.quizsnapshotid,
                        principalTable: "quizsnapshots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_quizattempts_quiztemplates_quiztemplateid",
                        column: x => x.quiztemplateid,
                        principalTable: "quiztemplates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quizsnapshotquestionsnapshotmaps",
                columns: table => new
                {
                    quizsnapshotid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    questionsnapshotid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quizsnapshotquestionsnapshotmaps", x => new { x.quizsnapshotid, x.questionsnapshotid });
                    table.ForeignKey(
                        name: "FK_quizsnapshotquestionsnapshotmaps_questionsnapshots_question~",
                        column: x => x.questionsnapshotid,
                        principalTable: "questionsnapshots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_quizsnapshotquestionsnapshotmaps_quizsnapshots_quizsnapshot~",
                        column: x => x.quizsnapshotid,
                        principalTable: "quizsnapshots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "questionrecords",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    markupcontent = table.Column<string>(type: "text", nullable: false),
                    iscorrect = table.Column<bool>(type: "boolean", nullable: false),
                    quizattemptid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    sortorder = table.Column<int>(type: "integer", nullable: false),
                    questionsnapshotid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questionrecords", x => x.id);
                    table.ForeignKey(
                        name: "FK_questionrecords_questionsnapshots_questionsnapshotid",
                        column: x => x.questionsnapshotid,
                        principalTable: "questionsnapshots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_questionrecords_quizattempts_quizattemptid",
                        column: x => x.quizattemptid,
                        principalTable: "quizattempts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "answerrecords",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    quizattemptid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    questionrecordid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_answerrecords", x => x.id);
                    table.ForeignKey(
                        name: "FK_answerrecords_questionrecords_questionrecordid",
                        column: x => x.questionrecordid,
                        principalTable: "questionrecords",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_answerrecords_quizattempts_quizattemptid",
                        column: x => x.quizattemptid,
                        principalTable: "quizattempts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_answerrecords_questionrecordid",
                table: "answerrecords",
                column: "questionrecordid");

            migrationBuilder.CreateIndex(
                name: "IX_answerrecords_quizattemptid",
                table: "answerrecords",
                column: "quizattemptid");

            migrationBuilder.CreateIndex(
                name: "IX_questionanswersnapshots_questionsnapshotid_sortorder",
                table: "questionanswersnapshots",
                columns: new[] { "questionsnapshotid", "sortorder" });

            migrationBuilder.CreateIndex(
                name: "IX_questionrecords_questionsnapshotid",
                table: "questionrecords",
                column: "questionsnapshotid");

            migrationBuilder.CreateIndex(
                name: "IX_questionrecords_quizattemptid_sortorder",
                table: "questionrecords",
                columns: new[] { "quizattemptid", "sortorder" });

            migrationBuilder.CreateIndex(
                name: "IX_questionselectionrules_quizetemplateid",
                table: "questionselectionrules",
                column: "quizetemplateid");

            migrationBuilder.CreateIndex(
                name: "IX_questionsnapshots_groupid_type_level",
                table: "questionsnapshots",
                columns: new[] { "groupid", "type", "level" });

            migrationBuilder.CreateIndex(
                name: "IX_questionsnapshots_questionid",
                table: "questionsnapshots",
                column: "questionid");

            migrationBuilder.CreateIndex(
                name: "IX_questionsnapshots_quiztemplateid",
                table: "questionsnapshots",
                column: "quiztemplateid");

            migrationBuilder.CreateIndex(
                name: "IX_quizattempts_assignid",
                table: "quizattempts",
                column: "assignid",
                filter: "AssignId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_quizattempts_quizsnapshotid",
                table: "quizattempts",
                column: "quizsnapshotid");

            migrationBuilder.CreateIndex(
                name: "IX_quizattempts_quiztemplateid_status",
                table: "quizattempts",
                columns: new[] { "quiztemplateid", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_quizsnapshotquestionsnapshotmaps_questionsnapshotid",
                table: "quizsnapshotquestionsnapshotmaps",
                column: "questionsnapshotid");

            migrationBuilder.CreateIndex(
                name: "IX_quizsnapshots_quiztemplateid",
                table: "quizsnapshots",
                column: "quiztemplateid");

            migrationBuilder.CreateIndex(
                name: "IX_quiztemplates_ownerid_ispublished",
                table: "quiztemplates",
                columns: new[] { "ownerid", "ispublished" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "answerrecords");

            migrationBuilder.DropTable(
                name: "questionanswersnapshots");

            migrationBuilder.DropTable(
                name: "questionselectionrules");

            migrationBuilder.DropTable(
                name: "quizsnapshotquestionsnapshotmaps");

            migrationBuilder.DropTable(
                name: "questionrecords");

            migrationBuilder.DropTable(
                name: "questionsnapshots");

            migrationBuilder.DropTable(
                name: "quizattempts");

            migrationBuilder.DropTable(
                name: "quizsnapshots");

            migrationBuilder.DropTable(
                name: "quiztemplates");
        }
    }
}
