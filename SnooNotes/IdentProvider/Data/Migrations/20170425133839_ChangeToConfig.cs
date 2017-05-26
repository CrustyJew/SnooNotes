using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IdentProvider.Data.Migrations
{
    public partial class ChangeToConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HasRead",
                table: "AspNetUsers",
                newName: "HasConfig");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HasConfig",
                table: "AspNetUsers",
                newName: "HasRead");
        }
    }
}
