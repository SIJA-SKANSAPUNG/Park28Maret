using System;

namespace ParkIRC.Models
{
    public class VehicleEntryRequest
    {
        public int EntryGateId { get; set; }
        public string VehicleNumber { get; set; }
        public string VehicleType { get; set; }
        public string ImagePath { get; set; }
    }
}
