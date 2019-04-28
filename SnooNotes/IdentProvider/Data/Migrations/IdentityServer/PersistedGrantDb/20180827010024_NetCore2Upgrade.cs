using Microsoft.EntityFrameworkCore.Migrations;

namespace IdentProvider.Data.Migrations.IdentityServer.PersistedGrantDb
{
    public partial class NetCore2Upgrade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PersistedGrants",
                table: "PersistedGrants");

            migrationBuilder.DropIndex(
                name: "IX_PersistedGrants_SubjectId",
                table: "PersistedGrants");

            migrationBuilder.DropIndex(
                name: "IX_PersistedGrants_SubjectId_ClientId",
                table: "PersistedGrants");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersistedGrants",
                table: "PersistedGrants",
                column: "Key");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PersistedGrants",
                table: "PersistedGrants");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersistedGrants",
                table: "PersistedGrants",
                columns: new[] { "Key", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_PersistedGrants_SubjectId",
                table: "PersistedGrants",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PersistedGrants_SubjectId_ClientId",
                table: "PersistedGrants",
                columns: new[] { "SubjectId", "ClientId" });
        }
    }
}
