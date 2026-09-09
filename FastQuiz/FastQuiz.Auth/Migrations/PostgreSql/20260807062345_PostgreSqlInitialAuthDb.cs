using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FastQuiz.Auth.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class PostgreSqlInitialAuthDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "rolepermissions",
                columns: table => new
                {
                    permission = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    roleid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    createdat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rolepermissions", x => new { x.roleid, x.permission });
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    createdat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "citext", maxLength: 256, nullable: true),
                    passwordhash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    createdat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organizations",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    tenantid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    parentid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    createdat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizations", x => x.id);
                    table.ForeignKey(
                        name: "FK_organizations_organizations_parentid",
                        column: x => x.parentid,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organizations_tenants_tenantid",
                        column: x => x.tenantid,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    tenantid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    issystem = table.Column<bool>(type: "boolean", nullable: false),
                    isenabled = table.Column<bool>(type: "boolean", nullable: false),
                    createdat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                    table.ForeignKey(
                        name: "FK_roles_tenants_tenantid",
                        column: x => x.tenantid,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenantusermaps",
                columns: table => new
                {
                    tenantid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    userid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    createdat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenantusermaps", x => new { x.tenantid, x.userid });
                    table.ForeignKey(
                        name: "FK_tenantusermaps_tenants_tenantid",
                        column: x => x.tenantid,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenantusermaps_users_userid",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organizationusermaps",
                columns: table => new
                {
                    organizationid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    userid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    createdat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizationusermaps", x => new { x.organizationid, x.userid });
                    table.ForeignKey(
                        name: "FK_organizationusermaps_organizations_organizationid",
                        column: x => x.organizationid,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organizationusermaps_users_userid",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "userroles",
                columns: table => new
                {
                    userid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    roleid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    tenantid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    createdat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userroles", x => new { x.userid, x.roleid, x.tenantid });
                    table.ForeignKey(
                        name: "FK_userroles_roles_roleid",
                        column: x => x.roleid,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_userroles_users_userid",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Organization_ParentId",
                table: "organizations",
                column: "parentid");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_TenantId",
                table: "organizations",
                column: "tenantid");

            migrationBuilder.CreateIndex(
                name: "IX_organizationusermaps_organizationid",
                table: "organizationusermaps",
                column: "organizationid");

            migrationBuilder.CreateIndex(
                name: "IX_organizationusermaps_userid",
                table: "organizationusermaps",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_roles_tenantid_name",
                table: "roles",
                columns: new[] { "tenantid", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenantusermaps_tenantid",
                table: "tenantusermaps",
                column: "tenantid");

            migrationBuilder.CreateIndex(
                name: "IX_tenantusermaps_userid",
                table: "tenantusermaps",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_userroles_roleid",
                table: "userroles",
                column: "roleid");

            migrationBuilder.CreateIndex(
                name: "IX_userroles_tenantid",
                table: "userroles",
                column: "tenantid");

            migrationBuilder.CreateIndex(
                name: "IX_userroles_userid",
                table: "userroles",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true,
                filter: "email IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "organizationusermaps");

            migrationBuilder.DropTable(
                name: "rolepermissions");

            migrationBuilder.DropTable(
                name: "tenantusermaps");

            migrationBuilder.DropTable(
                name: "userroles");

            migrationBuilder.DropTable(
                name: "organizations");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "tenants");
        }
    }
}
