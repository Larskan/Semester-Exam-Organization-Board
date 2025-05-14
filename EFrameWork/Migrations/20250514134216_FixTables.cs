using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFrameWork.Migrations
{
    /// <inheritdoc />
    public partial class FixTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "RoleTables",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "RoleTables",
                keyColumn: "RoleID",
                keyValue: 1,
                column: "Role",
                value: "Admin");

            migrationBuilder.UpdateData(
                table: "RoleTables",
                keyColumn: "RoleID",
                keyValue: 2,
                column: "Role",
                value: "Team Leader");

            migrationBuilder.UpdateData(
                table: "RoleTables",
                keyColumn: "RoleID",
                keyValue: 3,
                column: "Role",
                value: "Team Member");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "RoleTables",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "RoleTables",
                keyColumn: "RoleID",
                keyValue: 1,
                column: "Role",
                value: 0);

            migrationBuilder.UpdateData(
                table: "RoleTables",
                keyColumn: "RoleID",
                keyValue: 2,
                column: "Role",
                value: 1);

            migrationBuilder.UpdateData(
                table: "RoleTables",
                keyColumn: "RoleID",
                keyValue: 3,
                column: "Role",
                value: 2);
        }
    }
}
