using System;
using System.ComponentModel.DataAnnotations;

namespace ParkIRC.Models
{
    public class EntryGate
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Location { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public bool IsOnline { get; set; } = true;
        
        public bool IsOpen { get; set; } = false;
        
        public DateTime LastActivityTime { get; set; } = DateTime.Now;
        
        public DateTime? LastActivity { get; set; }
        
        public string Status => IsActive ? (IsOnline ? "Online" : "Offline") : "Disabled";
        
        public string IpAddress { get; set; } = string.Empty;
        
        public int PortNumber { get; set; }
        
        public string Description { get; set; } = string.Empty;
    }
} 