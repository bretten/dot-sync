using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.StorageLocations.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public partial class InitialCreateStorageLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "storage_locations");

            migrationBuilder.CreateTable(
                name: "storage_locations",
                schema: "storage_locations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "varchar(12)", nullable: false),
                    path = table.Column<string>(type: "text", nullable: false),
                    file_count = table.Column<long>(type: "bigint", nullable: false),
                    size = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("storage_locations_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "historical_storage_statistics",
                schema: "storage_locations",
                columns: table => new
                {
                    storage_location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    file_count = table.Column<long>(type: "bigint", nullable: false),
                    size = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("historical_storage_statistics_pkey", x => new { x.storage_location_id, x.timestamp });
                    table.ForeignKey(
                        name: "historical_storage_statistics_storage_locations_id_fkey",
                        column: x => x.storage_location_id,
                        principalSchema: "storage_locations",
                        principalTable: "storage_locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "storage_locations_path_idx",
                schema: "storage_locations",
                table: "storage_locations",
                column: "path",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historical_storage_statistics",
                schema: "storage_locations");

            migrationBuilder.DropTable(
                name: "storage_locations",
                schema: "storage_locations");
        }
    }
}
