using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangedUser8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleType",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "RoleType",
                table: "Users",
                newName: "AssignedRoleRoleType");

            migrationBuilder.RenameIndex(
                name: "IX_Users_RoleType",
                table: "Users",
                newName: "IX_Users_AssignedRoleRoleType");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_AssignedRoleRoleType",
                table: "Users",
                column: "AssignedRoleRoleType",
                principalTable: "Roles",
                principalColumn: "RoleType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_AssignedRoleRoleType",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "AssignedRoleRoleType",
                table: "Users",
                newName: "RoleType");

            migrationBuilder.RenameIndex(
                name: "IX_Users_AssignedRoleRoleType",
                table: "Users",
                newName: "IX_Users_RoleType");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleType",
                table: "Users",
                column: "RoleType",
                principalTable: "Roles",
                principalColumn: "RoleType");
        }
    }
}
