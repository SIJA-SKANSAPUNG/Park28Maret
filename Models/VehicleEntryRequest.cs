using System;

namespace ParkIRC.Models
{
    public class VehicleEntryRequest
    {
        public string VehicleNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public DateTime EntryTime { get; set; }
        public string GateId { get; set; } = string.Empty; 
        public string EntryGateId { get; set; } = string.Empty; 
        public string OperatorId { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string VehicleTypeId { get; set; } = string.Empty;
        public string ParkingSpotId { get; set; } = string.Empty;
    }
}
