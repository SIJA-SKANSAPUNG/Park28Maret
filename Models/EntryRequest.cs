using System;

namespace ParkIRC.Models
{
    public class EntryRequest
    {
        public string VehicleNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string GateId { get; set; } = string.Empty;
        public string OperatorId { get; set; } = string.Empty;
        public string ImageData { get; set; } = string.Empty;
    }
}