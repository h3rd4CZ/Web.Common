using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RhDev.Customer.Component.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class AuditTrails_Refs_Cascading2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Cities");

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedById",
                table: "Cities",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cities_LastModifiedById",
                table: "Cities",
                column: "LastModifiedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_AspNetUsers_LastModifiedById",
                table: "Cities",
                column: "LastModifiedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cities_AspNetUsers_LastModifiedById",
                table: "Cities");

            migrationBuilder.DropIndex(
                name: "IX_Cities_LastModifiedById",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "LastModifiedById",
                table: "Cities");

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Cities",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
