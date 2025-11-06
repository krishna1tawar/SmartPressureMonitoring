namespace SensoreApp.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CommentText { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}