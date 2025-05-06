using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangedUser2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleUser");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "RoleUser",
                columns: table => new
                {
                    AssignedRolesRoleType = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleUser", x => new { x.AssignedRolesRoleType, x.UserId });
                    table.ForeignKey(
                        name: "FK_RoleUser_Roles_AssignedRolesRoleType",
                        column: x => x.AssignedRolesRoleType,
                        principalTable: "Roles",
                        principalColumn: "RoleType",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleUser_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoleUser_UserId",
                table: "RoleUser",
                column: "UserId");
        }
    }
}
