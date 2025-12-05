using System;

namespace Sensore_Project.Models
{
    /// <summary>
    /// Represents a comment on an alert, with optional clinician feedback.
    /// </summary>
    public class Comment
    {
        public int Id { get; set; }

        /// <summary>Foreign key to the parent alert.</summary>
        public int AlertId { get; set; }

        /// <summary>Navigation property to the parent alert.</summary>
        public Alert? Alert { get; set; }

        /// <summary>User who created the comment.</summary>
        public int UserId { get; set; }

        public string CommentText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Optional feedback from a clinician.</summary>
        public string? FeedbackText { get; set; }

        public DateTime? FeedbackProvidedAt { get; set; }

        /// <summary>User who provided the feedback.</summary>
        public int? FeedbackUserId { get; set; }
    }
}