using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RhDev.Customer.Component.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApplicationUserSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Changed = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Population = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DayOffs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Day = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Repeat = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayOffs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DayOffs",
                columns: new[] { "Id", "Day", "Repeat", "Title" },
                values: new object[,]
                {
                    { 1, new DateTime(2022, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Den obnovy samostatného českého státu" },
                    { 2, new DateTime(2022, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Svátek práce" },
                    { 3, new DateTime(2022, 5, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Den vítězství" },
                    { 5, new DateTime(2022, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Den slovanských věrozvěstů Cyrila a Metoděje" },
                    { 6, new DateTime(2022, 7, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Den upálení mistra Jana Husa" },
                    { 7, new DateTime(2022, 9, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Den české státnosti" },
                    { 8, new DateTime(2022, 11, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Den boje za svobodu a demokracii" },
                    { 9, new DateTime(2022, 12, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Štědrý den" },
                    { 10, new DateTime(2022, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "1. svátek vánoční" },
                    { 11, new DateTime(2022, 12, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "2. svátek vánoční" },
                    { 12, new DateTime(2022, 12, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "2. svátek vánoční" },
                    { 13, new DateTime(2023, 4, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Velký pátek" },
                    { 14, new DateTime(2024, 3, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Velký pátek" },
                    { 15, new DateTime(2025, 4, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Velký pátek" },
                    { 16, new DateTime(2026, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Velký pátek" },
                    { 17, new DateTime(2027, 3, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Velký pátek" },
                    { 18, new DateTime(2028, 4, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Velký pátek" },
                    { 19, new DateTime(2029, 3, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Velký pátek" },
                    { 20, new DateTime(2030, 4, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Velký pátek" },
                    { 21, new DateTime(2023, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Velikonoční pondělí" },
                    { 22, new DateTime(2024, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Velikonoční pondělí" },
                    { 23, new DateTime(2025, 4, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Velikonoční pondělí" },
                    { 24, new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Velikonoční pondělí" },
                    { 25, new DateTime(2027, 3, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Velikonoční pondělí" },
                    { 26, new DateTime(2028, 4, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Velikonoční pondělí" },
                    { 27, new DateTime(2029, 4, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Velikonoční pondělí" },
                    { 28, new DateTime(2030, 4, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Velikonoční pondělí" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Key",
                table: "ApplicationUserSettings",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationUserSettings");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "DayOffs");

            migrationBuilder.DropColumn(
                name: "City",
                table: "AspNetUsers");
        }
    }
}
