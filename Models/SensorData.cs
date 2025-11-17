using System;
using Sensore_Project.Models;
namespace Sensore_Project.Models
{
    public class SensorData
    {
        public int Id { get; set; }
        public double Pressure { get; set; }
        public DateTime Timestamp { get; set; }

        // ML anomaly detection fields
        public bool IsAnomalous { get; set; }
        public double AnomalyScore { get; set; }
    }
}