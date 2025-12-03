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

        public DateTime? Timestamp { get; set; }
    }
}