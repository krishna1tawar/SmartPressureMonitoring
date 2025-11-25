namespace Sensore_Project.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string CommentText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}