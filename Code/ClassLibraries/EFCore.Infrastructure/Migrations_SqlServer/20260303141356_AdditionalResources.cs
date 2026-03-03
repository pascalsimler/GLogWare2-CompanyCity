using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Gudel.GLogWare.EFCore.Infrastructure.Migrations_SqlServer
{
    /// <inheritdoc />
    public partial class AdditionalResources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Resources",
                columns: new[] { "Name", "ErrorFlag", "InfeedEnabled", "Mode", "Occupied", "OutfeedEnabled", "Parked", "RelocationEnabled" },
                values: new object[,]
                {
                    { "OP7300BR", true, true, "UNDEFINED", false, true, false, true },
                    { "OP7400BR", true, true, "UNDEFINED", false, true, false, true },
                    { "OP7500BR", true, true, "UNDEFINED", false, true, false, true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Name",
                keyValue: "OP7300BR");

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Name",
                keyValue: "OP7400BR");

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Name",
                keyValue: "OP7500BR");
        }
    }
}
