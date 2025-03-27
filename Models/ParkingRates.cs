using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkIRC.Models
{
    // This class is an alias for ParkingRateConfiguration for backward compatibility
    public class ParkingRates
    {
        public ParkingRates()
        {
            // Default constructor for backward compatibility
        }

        // Create from ParkingRateConfiguration
        public ParkingRates(ParkingRateConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            Id = config.Id;
            VehicleType = config.VehicleType;
            HourlyRate = config.HourlyRate;
            DailyRate = config.DailyRate;
            WeeklyRate = config.WeeklyRate;
            MonthlyRate = config.MonthlyRate;
            IsActive = config.IsActive;
        }

        // Convert to ParkingRateConfiguration
        public ParkingRateConfiguration ToConfiguration()
        {
            return new ParkingRateConfiguration
            {
                Id = Id,
                VehicleType = VehicleType,
                HourlyRate = HourlyRate,
                DailyRate = DailyRate,
                WeeklyRate = WeeklyRate,
                MonthlyRate = MonthlyRate,
                IsActive = IsActive,
                BaseRate = BaseRate,
                PenaltyRate = PenaltyRate,
                EffectiveFrom = DateTime.Now,
                CreatedBy = "System",
                LastModifiedBy = "System",
                MotorcycleRate = MotorcycleRate,
                CarRate = CarRate,
                AdditionalHourRate = AdditionalHourRate,
                MaximumDailyRate = MaximumDailyRate,
                LastUpdated = DateTime.Now,
                UpdatedBy = UpdatedBy
            };
        }

        public int Id { get; set; }
        
        [Required]
        public string VehicleType { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal DailyRate { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal WeeklyRate { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyRate { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseRate { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal PenaltyRate { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal MotorcycleRate { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal CarRate { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal AdditionalHourRate { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal MaximumDailyRate { get; set; }
        
        public string? UpdatedBy { get; set; }
    }
} 