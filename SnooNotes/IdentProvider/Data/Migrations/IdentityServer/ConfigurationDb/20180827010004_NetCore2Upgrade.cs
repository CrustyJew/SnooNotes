using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IdentProvider.Data.Migrations.IdentityServer.ConfigurationDb
{
    public partial class NetCore2Upgrade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoutUri",
                table: "Clients");

            migrationBuilder.RenameColumn(
                name: "PrefixClientClaims",
                table: "Clients",
                newName: "FrontChannelLogoutSessionRequired");

            migrationBuilder.RenameColumn(
                name: "LogoutSessionRequired",
                table: "Clients",
                newName: "BackChannelLogoutSessionRequired");

            migrationBuilder.AlterColumn<string>(
                name: "LogoUri",
                table: "Clients",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BackChannelLogoutUri",
                table: "Clients",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientClaimsPrefix",
                table: "Clients",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConsentLifetime",
                table: "Clients",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Clients",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FrontChannelLogoutUri",
                table: "Clients",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PairWiseSubjectSalt",
                table: "Clients",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClientProperties",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(maxLength: 250, nullable: false),
                    Value = table.Column<string>(maxLength: 2000, nullable: false),
                    ClientId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientProperties_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientProperties_ClientId",
                table: "ClientProperties",
                column: "ClientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientProperties");

            migrationBuilder.DropColumn(
                name: "BackChannelLogoutUri",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "ClientClaimsPrefix",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "ConsentLifetime",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "FrontChannelLogoutUri",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PairWiseSubjectSalt",
                table: "Clients");

            migrationBuilder.RenameColumn(
                name: "FrontChannelLogoutSessionRequired",
                table: "Clients",
                newName: "PrefixClientClaims");

            migrationBuilder.RenameColumn(
                name: "BackChannelLogoutSessionRequired",
                table: "Clients",
                newName: "LogoutSessionRequired");

            migrationBuilder.AlterColumn<string>(
                name: "LogoUri",
                table: "Clients",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoutUri",
                table: "Clients",
                nullable: true);
        }
    }
}
