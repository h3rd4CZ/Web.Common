using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RhDev.Customer.Component.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class AuditTrails_Refs_Cascading : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Cities");

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Cities",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cities_CreatedById",
                table: "Cities",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_AspNetUsers_CreatedById",
                table: "Cities",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cities_AspNetUsers_CreatedById",
                table: "Cities");

            migrationBuilder.DropIndex(
                name: "IX_Cities_CreatedById",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Cities");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Cities",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
