using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RhDev.Customer.Component.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConcurrencyStampCity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "Cities",
                type: "rowversion",
                rowVersion: true,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "Cities");
        }
    }
}
