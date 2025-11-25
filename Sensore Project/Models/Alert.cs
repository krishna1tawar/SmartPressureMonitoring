using System;

namespace Sensore_Project.Models
{
    public class Alert
    {
        public int Id { get; set; }

        public int UserId { get; set; } = 1;   // optional, placeholder for future authentication

        public string Message { get; set; } = string.Empty;

        public double Pressure { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public bool IsResolved { get; set; }
    }
}