﻿namespace Kongsberg_Assignment
{
    /// <summary>
    /// Sensor Configuration parsed from sensor config.
    /// </summary>
    public class Sensor
    {
        public int ID { get; set; }
        public string Type { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public int Frequency { get; set; }
    }
}