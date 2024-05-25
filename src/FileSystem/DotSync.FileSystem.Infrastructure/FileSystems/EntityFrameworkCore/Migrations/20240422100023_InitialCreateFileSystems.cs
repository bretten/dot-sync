using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public partial class InitialCreateFileSystems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "file_systems");

            migrationBuilder.CreateTable(
                name: "files",
                schema: "file_systems",
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

            migrationBuilder.CreateIndex(
                name: "files_path_idx",
                schema: "file_systems",
                table: "files",
                column: "path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "files_sha256_checksum_idx",
                schema: "file_systems",
                table: "files",
                column: "sha256_checksum",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "files",
                schema: "file_systems");
        }
    }
}
