using Microsoft.EntityFrameworkCore.Migrations;

namespace RemoteSchoolWebApp.Migrations
{
    public partial class UpdatingModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "Parents");

            migrationBuilder.RenameColumn(
                name: "Login",
                table: "Teachers",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "Login",
                table: "Parents",
                newName: "Email");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Teachers",
                newName: "Login");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Parents",
                newName: "Login");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Teachers",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Parents",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");
        }
    }
}
