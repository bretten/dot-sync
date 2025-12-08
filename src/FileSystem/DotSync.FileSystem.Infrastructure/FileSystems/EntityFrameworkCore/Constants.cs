using com.brettnamba.DotSync.FileSystem.Domain.FileSystems.Entities;

namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;

public static class Constants
{
    /// <summary>
    /// The schema name
    /// </summary>
    public const string Schema = "file_systems";

    /// <summary>
    /// Constants for <see cref="DotFile"/>
    /// </summary>
    public static class Files
    {
        /// <summary>
        /// Table name
        /// </summary>
        public const string TableName = "files";

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

        /// <summary>
        /// Key names for <see cref="DotFile"/>
        /// </summary>
        public static class Keys
        {
            /// <summary>
            /// Primary key
            /// </summary>
            public const string PrimaryKey = $"{TableName}_pkey";
        }

        /// <summary>
        /// Index names for <see cref="DotFile"/>
        /// </summary>
        public static class Indexes
        {
            /// <summary>
            /// Index on the file checksum
            /// </summary>
            public const string Sha256ChecksumIndex = $"{TableName}_{Sha256Checksum}_idx";

            /// <summary>
            /// Index on the file path
            /// </summary>
            public const string PathIndex = $"{TableName}_{Path}_idx";
        }
    }

    /// <summary>
    /// Constants for <see cref="StorageLocation"/>
    /// </summary>
    public static class StorageLocations
    {
        /// <summary>
        /// Table name
        /// </summary>
        public const string TableName = "storage_locations";

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
        /// Key names for <see cref="StorageLocation"/>
        /// </summary>
        public static class Keys
        {
            /// <summary>
            /// Primary key
            /// </summary>
            public const string PrimaryKey = $"{TableName}_pkey";
        }

        /// <summary>
        /// Index names for <see cref="StorageLocation"/>
        /// </summary>
        public static class Indexes
        {
            /// <summary>
            /// Index on storage path
            /// </summary>
            public const string PathIndex = $"{TableName}_{Path}_idx";
        }
    }

    /// <summary>
    /// Constants for <see cref="SyncedFile"/>
    /// </summary>
    public static class SyncedFiles
    {
        /// <summary>
        /// Table name
        /// </summary>
        public const string TableName = "synced_files";

        /// <summary>
        /// File ID
        /// </summary>
        public const string FileId = "file_id";

        /// <summary>
        /// Storage Location ID
        /// </summary>
        public const string StoreLocationId = "storage_location_id";

        /// <summary>
        /// Last sync
        /// </summary>
        public const string LastSync = "last_sync";

        /// <summary>
        /// Key names for <see cref="SyncedFile"/>
        /// </summary>
        public static class Keys
        {
            /// <summary>
            /// Primary key
            /// </summary>
            public const string PrimaryKey = $"{TableName}_pkey";

            /// <summary>
            /// <see cref="SyncedFile"/> references <see cref="DotFile"/>
            /// </summary>
            public const string FileForeignKeyConstraint = $"{TableName}_{Files.TableName}_{Files.Id}_fkey";

            /// <summary>
            /// <see cref="SyncedFile"/> references <see cref="StorageLocation"/>
            /// </summary>
            public const string StorageLocationForeignKeyConstraint =
                $"{TableName}_{StorageLocations.TableName}_{StorageLocations.Id}_fkey";
        }

        /// <summary>
        /// Index names for <see cref="SyncedFile"/>
        /// </summary>
        public static class Indexes
        {
            /// <summary>
            /// Index for the trailing column in the composite PK
            /// </summary>
            public const string StorageLocation = $"{TableName}_{StoreLocationId}_idx";
        }
    }
}