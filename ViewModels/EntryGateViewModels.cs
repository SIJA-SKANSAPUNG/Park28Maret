using System;
using System.Collections.Generic;
using ParkIRC.Models;

namespace ParkIRC.ViewModels
{
    public class EntryGateViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public CameraSettings CameraSettings { get; set; } = new CameraSettings();
    }

    public class EntryGateActivity
    {
        public DateTime Timestamp { get; set; }
        public string ActivityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
} 