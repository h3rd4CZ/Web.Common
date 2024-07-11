using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RhDev.Customer.Component.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class AuditTrails_Refs_Cascading3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cities_AspNetUsers_CreatedById",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_Cities_AspNetUsers_LastModifiedById",
                table: "Cities");

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_AspNetUsers_CreatedById",
                table: "Cities",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_AspNetUsers_LastModifiedById",
                table: "Cities",
                column: "LastModifiedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cities_AspNetUsers_CreatedById",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_Cities_AspNetUsers_LastModifiedById",
                table: "Cities");

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_AspNetUsers_CreatedById",
                table: "Cities",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_AspNetUsers_LastModifiedById",
                table: "Cities",
                column: "LastModifiedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
