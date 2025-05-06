using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangedUser3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_AssignedRoleRoleType",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_AssignedRoleRoleType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AssignedRoleRoleType",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "RoleType",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleType",
                table: "Users",
                column: "RoleType");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleType",
                table: "Users",
                column: "RoleType",
                principalTable: "Roles",
                principalColumn: "RoleType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleType",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_RoleType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RoleType",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "AssignedRoleRoleType",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Users_AssignedRoleRoleType",
                table: "Users",
                column: "AssignedRoleRoleType");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_AssignedRoleRoleType",
                table: "Users",
                column: "AssignedRoleRoleType",
                principalTable: "Roles",
                principalColumn: "RoleType",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
