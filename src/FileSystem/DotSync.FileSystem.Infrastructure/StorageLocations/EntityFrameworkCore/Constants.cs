using com.brettnamba.DotSync.FileSystem.Domain.StorageLocations.Entities;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.StorageLocations.EntityFrameworkCore;

public static class Constants
{
    /// <summary>
    /// The schema name
    /// </summary>
    public const string Schema = "storage_locations";

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
}