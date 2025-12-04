using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public partial class MoveStorageLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "storage_locations",
                schema: "file_systems",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "varchar(12)", nullable: false),
                    path = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("storage_locations_pkey", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "storage_locations_path_idx",
                schema: "file_systems",
                table: "storage_locations",
                column: "path",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "storage_locations",
                schema: "file_systems");
        }
    }
}
