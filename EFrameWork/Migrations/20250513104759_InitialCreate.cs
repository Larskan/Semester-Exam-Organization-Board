using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EFrameWork.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrganizationTables",
                columns: table => new
                {
                    OrganizationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationTables", x => x.OrganizationID);
                });

            migrationBuilder.CreateTable(
                name: "RoleTables",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleTables", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "StatusTables",
                columns: table => new
                {
                    StatusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusTables", x => x.StatusID);
                });

            migrationBuilder.CreateTable(
                name: "TeamTables",
                columns: table => new
                {
                    TeamID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamTables", x => x.TeamID);
                });

            migrationBuilder.CreateTable(
                name: "BoardTables",
                columns: table => new
                {
                    BoardID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoardName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeamID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardTables", x => x.BoardID);
                    table.ForeignKey(
                        name: "FK_BoardTables_TeamTables_TeamID",
                        column: x => x.TeamID,
                        principalTable: "TeamTables",
                        principalColumn: "TeamID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTables",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleID = table.Column<int>(type: "int", nullable: false),
                    OrganizationID = table.Column<int>(type: "int", nullable: false),
                    TeamID = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTables", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_UserTables_OrganizationTables_OrganizationID",
                        column: x => x.OrganizationID,
                        principalTable: "OrganizationTables",
                        principalColumn: "OrganizationID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTables_RoleTables_RoleID",
                        column: x => x.RoleID,
                        principalTable: "RoleTables",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTables_TeamTables_TeamID",
                        column: x => x.TeamID,
                        principalTable: "TeamTables",
                        principalColumn: "TeamID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskTables",
                columns: table => new
                {
                    TaskID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoardID = table.Column<int>(type: "int", nullable: false),
                    StatusID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estimation = table.Column<float>(type: "real", nullable: true),
                    NumUser = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTables", x => x.TaskID);
                    table.ForeignKey(
                        name: "FK_TaskTables_BoardTables_BoardID",
                        column: x => x.BoardID,
                        principalTable: "BoardTables",
                        principalColumn: "BoardID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskTables_StatusTables_StatusID",
                        column: x => x.StatusID,
                        principalTable: "StatusTables",
                        principalColumn: "StatusID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserToTaskTables",
                columns: table => new
                {
                    UserToTaskID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    TaskID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserToTaskTables", x => x.UserToTaskID);
                    table.ForeignKey(
                        name: "FK_UserToTaskTables_TaskTables_TaskID",
                        column: x => x.TaskID,
                        principalTable: "TaskTables",
                        principalColumn: "TaskID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserToTaskTables_UserTables_UserID",
                        column: x => x.UserID,
                        principalTable: "UserTables",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "OrganizationTables",
                columns: new[] { "OrganizationID", "OrganizationName" },
                values: new object[] { 1, "Lars" });

            migrationBuilder.InsertData(
                table: "RoleTables",
                columns: new[] { "RoleID", "Role" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "Team Lead" },
                    { 3, "Team Member" }
                });

            migrationBuilder.InsertData(
                table: "TeamTables",
                columns: new[] { "TeamID", "TeamName" },
                values: new object[,]
                {
                    { 1, "team 1" },
                    { 2, "team 2" }
                });

            migrationBuilder.InsertData(
                table: "UserTables",
                columns: new[] { "UserID", "Email", "OrganizationID", "Password", "RoleID", "TeamID" },
                values: new object[,]
                {
                    { 1, "Mail1", 1, "1234", 1, 1 },
                    { 2, "Mail1", 1, "1234", 2, 2 },
                    { 3, "Mail1", 1, "1234", 3, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoardTables_TeamID",
                table: "BoardTables",
                column: "TeamID");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTables_BoardID",
                table: "TaskTables",
                column: "BoardID");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTables_StatusID",
                table: "TaskTables",
                column: "StatusID");

            migrationBuilder.CreateIndex(
                name: "IX_UserTables_OrganizationID",
                table: "UserTables",
                column: "OrganizationID");

            migrationBuilder.CreateIndex(
                name: "IX_UserTables_RoleID",
                table: "UserTables",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_UserTables_TeamID",
                table: "UserTables",
                column: "TeamID");

            migrationBuilder.CreateIndex(
                name: "IX_UserToTaskTables_TaskID",
                table: "UserToTaskTables",
                column: "TaskID");

            migrationBuilder.CreateIndex(
                name: "IX_UserToTaskTables_UserID",
                table: "UserToTaskTables",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserToTaskTables");

            migrationBuilder.DropTable(
                name: "TaskTables");

            migrationBuilder.DropTable(
                name: "UserTables");

            migrationBuilder.DropTable(
                name: "BoardTables");

            migrationBuilder.DropTable(
                name: "StatusTables");

            migrationBuilder.DropTable(
                name: "OrganizationTables");

            migrationBuilder.DropTable(
                name: "RoleTables");

            migrationBuilder.DropTable(
                name: "TeamTables");
        }
    }
}
