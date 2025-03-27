using System;
using System.Collections.Generic;

namespace ParkIRC.ViewModels
{
    public class VehicleHistoryViewModel
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public DateTime? EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public TimeSpan? Duration { get; set; }
        public decimal? Amount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string TransactionNumber { get; set; } = string.Empty;
        public string ParkingSpaceNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }

    public class VehicleHistoryItemViewModel
    {
        public string TransactionNumber { get; set; } = string.Empty;
        public string TicketNumber { get; set; } = string.Empty;
        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime? PaymentTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string OperatorId { get; set; } = string.Empty;
        public string EntryPoint { get; set; } = string.Empty;
        public TimeSpan Duration => ExitTime.HasValue ? ExitTime.Value - EntryTime : TimeSpan.Zero;
    }
} 