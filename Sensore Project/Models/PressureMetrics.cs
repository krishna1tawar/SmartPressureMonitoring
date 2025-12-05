using System;
using System.Collections.Generic;

namespace Sensore_Project.Models
{
    /// <summary>
    /// Metrics computed from pressure map analysis.
    /// </summary>
    public class PressureMetrics
    {
        public List<HighPressureCluster> HighPressureClusters { get; set; } = new();
        public int TotalHighPressurePixels { get; set; }
        public double RiskScore { get; set; }
        public string RiskLevel { get; set; } = "Low";
        public bool AlertGenerated { get; set; }
        public DateTime? AlertTimestamp { get; set; }
        public bool HasBeenScanned { get; set; }
    }

    /// <summary>
    /// Represents a contiguous region of high-pressure pixels.
    /// </summary>
    public class HighPressureCluster
    {
        public int ClusterId { get; set; }
        public int PixelCount { get; set; }
        public int MaxPressure { get; set; }
        public double AvgPressure { get; set; }
        public BoundingBox BoundingBox { get; set; } = new();
        public Point Centroid { get; set; } = new();
    }

    /// <summary>
    /// Rectangular bounds of a cluster.
    /// </summary>
    public class BoundingBox
    {
        public int MinX { get; set; }
        public int MaxX { get; set; }
        public int MinY { get; set; }
        public int MaxY { get; set; }
    }

    /// <summary>
    /// 2D point coordinates.
    /// </summary>
    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
