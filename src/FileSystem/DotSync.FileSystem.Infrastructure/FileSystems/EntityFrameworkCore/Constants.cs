namespace com.brettnamba.DotSync.FileSystem.Infrastructure.FileSystems.EntityFrameworkCore;

public static class Constants
{
    /// <summary>
    /// The schema name
    /// </summary>
    public const string Schema = "file_systems";

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
        /// True if the file has been verified to have the correct path and checksum
        /// </summary>
        public const string IsVerified = "is_verified";

        /// <summary>
        /// Last update
        /// </summary>
        public const string UpdatedAt = "updated_at";

        /// <summary>
        /// When it was created
        /// </summary>
        public const string CreatedAt = "created_at";
    }
}