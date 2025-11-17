using System;

namespace Sensore_Project.Models
{
    public class Alert
    {
        public int Id { get; set; }

        // Optional – you can ignore this for now if you don't use users
        public int UserId { get; set; }

        public string Message { get; set; } = string.Empty;

        public double Pressure { get; set; }

        // When the alert was created
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public bool IsResolved { get; set; }
    }
}