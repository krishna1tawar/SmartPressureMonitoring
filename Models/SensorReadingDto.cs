using System;

namespace Sensore_Project.Models
{
    // This matches the JSON from the device:
    // [
    //   { "pressure": 30.5 },
    //   { "pressure": 31.2 }
    // ]
    public class SensorReadingDto
    {
        public double Pressure { get; set; }

        // Optional – if device ever sends its own timestamp.
        // If null, we'll use DateTime.UtcNow in the controller.
        public DateTime? Timestamp { get; set; }
    }
}