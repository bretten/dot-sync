using com.brettnamba.DotSync.FileSystem.Domain.FileStorage.Entities;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileStorage.EntityFrameworkCore;

public static class Constants
{
    /// <summary>
    /// The schema name
    /// </summary>
    public const string Schema = "file_storage";

    public static class Files
    {
        public const string TableName = "files";
        public const string PrimaryKey = $"{TableName}_pkey";
        public const string PathIndex = $"{TableName}_{Path}_idx";
        public const string Sha256ChecksumIndex = $"{TableName}_{Sha256Checksum}_idx";

        /// <summary>
        /// ID
        /// </summary>
        public const string Id = "id";

        /// <summary>
        /// Relative path to the file
        /// </summary>
        public const string Path = "path";

        /// <summary>
        /// Checksum of the file
        /// </summary>
        public const string Sha256Checksum = "sha256_checksum";

        /// <summary>
        /// Size of the file
        /// </summary>
        public const string Size = "size";

        /// <summary>
        /// The creation date of the file
        /// </summary>
        public const string FileCreation = "file_creation";

        /// <summary>
        /// True if the file has been verified to have the correct path and checksum
        /// </summary>
        public const string IsVerified = "is_verified";

        /// <summary>
        /// Last sync
        /// </summary>
        public const string LastSync = "last_sync";

        /// <summary>
        /// When the file was first synced
        /// </summary>
        public const string FirstSync = "first_sync";
    }

    public static class StorageLocations
    {
        public const string TableName = "storage_locations";
        public const string PrimaryKey = $"{TableName}_pkey";
        public const string PathIndex = $"{TableName}_{Path}_idx";

        /// <summary>
        /// ID
        /// </summary>
        public const string Id = "id";

        /// <summary>
        /// The path to the storage location
        /// </summary>
        public const string Path = "path";

        /// <summary>
        /// The type of storage location
        /// </summary>
        public const string Type = "type";

        /// <summary>
        /// The total number of files
        /// </summary>
        public const string FileCount = "file_count";

        /// <summary>
        /// The total size of the storage location
        /// </summary>
        public const string Size = "size";
    }

    public static class HistoricalStorageStatistics
    {
        public const string TableName = "historical_storage_statistics";
        public const string PrimaryKey = $"{TableName}_pkey";

        public const string StorageLocationsForeignKeyConstraint =
            $"{TableName}_{StorageLocations.TableName}_{StorageLocations.Id}_fkey";

        /// <summary>
        /// Column name that references <see cref="StorageLocation"/>
        /// </summary>
        public const string StorageLocationId = "storage_location_id";

        /// <summary>
        /// The timestamp of the statistics
        /// </summary>
        public const string Timestamp = "timestamp";

        /// <summary>
        /// The total number of files
        /// </summary>
        public const string FileCount = "file_count";

        /// <summary>
        /// The total size of the storage location
        /// </summary>
        public const string Size = "size";
    }

    public static class StoredFiles
    {
        public const string TableName = "stored_files";
        public const string FileForeignKey = "file_id";
        public const string StorageLocationForeignKey = "storage_location_id";
    }
}