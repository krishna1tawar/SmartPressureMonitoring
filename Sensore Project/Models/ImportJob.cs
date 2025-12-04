using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sensore_Project.Models
{
    /// <summary>
    /// Tracks the status and progress of CSV pressure map import jobs.
    /// </summary>
    public class ImportJob
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = ImportJobStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public int TotalFiles { get; set; }

        public int ProcessedFiles { get; set; }

        public int TotalMaps { get; set; }

        public int ProcessedMaps { get; set; }

        [MaxLength(255)]
        public string? CurrentFileName { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// JSON array of file names that have been successfully processed.
        /// Used to skip already-imported files on re-run.
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? ProcessedFilesList { get; set; }
    }

    /// <summary>
    /// Constants for ImportJob status values.
    /// </summary>
    public static class ImportJobStatus
    {
        public const string Pending = "Pending";
        public const string Processing = "Processing";
        public const string Completed = "Completed";
        public const string Failed = "Failed";
        public const string Cancelled = "Cancelled";
    }
}
