using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileStorage.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateFileStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "file_storage");

            migrationBuilder.CreateTable(
                name: "files",
                schema: "file_storage",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    path = table.Column<string>(type: "text", nullable: false),
                    sha256_checksum = table.Column<string>(type: "text", nullable: false),
                    size = table.Column<long>(type: "bigint", nullable: false),
                    file_creation = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    last_sync = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    first_sync = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("files_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "storage_locations",
                schema: "file_storage",
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
                schema: "file_storage",
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
                        principalSchema: "file_storage",
                        principalTable: "storage_locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stored_files",
                schema: "file_storage",
                columns: table => new
                {
                    file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    storage_location_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stored_files", x => new { x.file_id, x.storage_location_id });
                    table.ForeignKey(
                        name: "FK_stored_files_files_file_id",
                        column: x => x.file_id,
                        principalSchema: "file_storage",
                        principalTable: "files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_stored_files_storage_locations_storage_location_id",
                        column: x => x.storage_location_id,
                        principalSchema: "file_storage",
                        principalTable: "storage_locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "files_path_idx",
                schema: "file_storage",
                table: "files",
                column: "path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "files_sha256_checksum_idx",
                schema: "file_storage",
                table: "files",
                column: "sha256_checksum",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "storage_locations_path_idx",
                schema: "file_storage",
                table: "storage_locations",
                column: "path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stored_files_storage_location_id",
                schema: "file_storage",
                table: "stored_files",
                column: "storage_location_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historical_storage_statistics",
                schema: "file_storage");

            migrationBuilder.DropTable(
                name: "stored_files",
                schema: "file_storage");

            migrationBuilder.DropTable(
                name: "files",
                schema: "file_storage");

            migrationBuilder.DropTable(
                name: "storage_locations",
                schema: "file_storage");
        }
    }
}
