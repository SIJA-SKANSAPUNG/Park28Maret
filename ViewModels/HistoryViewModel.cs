using System;
using System.Collections.Generic;

namespace ParkIRC.ViewModels
{
    public class HistoryViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? VehicleNumber { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public dynamic? Transactions { get; set; }
    }

    public class HistoryTransactionViewModel
    {
        public string TransactionNumber { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public string Duration { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
