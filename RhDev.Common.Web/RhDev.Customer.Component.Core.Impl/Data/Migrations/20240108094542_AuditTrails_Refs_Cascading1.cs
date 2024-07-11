using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RhDev.Customer.Component.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class AuditTrails_Refs_Cascading1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cities_AspNetUsers_CreatedById",
                table: "Cities");

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_AspNetUsers_CreatedById",
                table: "Cities",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cities_AspNetUsers_CreatedById",
                table: "Cities");

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_AspNetUsers_CreatedById",
                table: "Cities",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
