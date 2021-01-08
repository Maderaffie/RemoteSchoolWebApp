﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace RemoteSchoolWebApp.Migrations
{
    public partial class class_debug : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teachers_ClassId",
                table: "Teachers");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_ClassId",
                table: "Teachers",
                column: "ClassId",
                unique: true,
                filter: "[ClassId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teachers_ClassId",
                table: "Teachers");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_ClassId",
                table: "Teachers",
                column: "ClassId");
        }
    }
}
