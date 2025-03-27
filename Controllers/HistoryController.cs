using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkIRC.Data;
using ParkIRC.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using ParkIRC.ViewModels;

namespace ParkIRC.Controllers
{
    [Authorize]
    public class HistoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HistoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10)
        {
            var transactions = _context.ParkingTransactions
                .Include(t => t.Vehicle)
                .Include(t => t.ParkingSpace)
                .AsQueryable();

            if (startDate.HasValue)
            {
                transactions = transactions.Where(t => t.EntryTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                transactions = transactions.Where(t => t.EntryTime <= endDate.Value.AddDays(1));
            }

            var totalCount = await transactions.CountAsync();

            var data = await transactions
                .OrderByDescending(t => t.EntryTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new 
                {
                    TransactionNumber = t.TransactionNumber,
                    VehicleNumber = t.Vehicle == null ? "Unknown" : t.Vehicle.VehicleNumber,
                    VehicleType = t.Vehicle == null ? "Unknown" : t.Vehicle.VehicleType,
                    EntryTime = t.EntryTime,
                    ExitTime = t.ExitTime,
                    Duration = t.ExitTime.HasValue ? 
                        $"{(int)(t.ExitTime.Value - t.EntryTime).TotalHours}h {(t.ExitTime.Value - t.EntryTime).Minutes}m" : 
                        "In Progress",
                    Amount = t.TotalAmount,
                    PaymentStatus = t.PaymentStatus,
                    PaymentMethod = t.PaymentMethod
                })
                .ToListAsync();

            var viewModel = new HistoryViewModel
            {
                Transactions = data,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.ParkingTransactions
                .Include(t => t.Vehicle)
                .Include(t => t.ParkingSpace)
                .FirstOrDefaultAsync(m => m.Id.ToString() == id);
            
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        public async Task<IActionResult> Export(DateTime? startDate = null, DateTime? endDate = null)
        {
            var transactions = _context.ParkingTransactions
                .Include(t => t.Vehicle)
                .Include(t => t.ParkingSpace)
                .AsQueryable();

            if (startDate.HasValue)
            {
                transactions = transactions.Where(t => t.EntryTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                transactions = transactions.Where(t => t.EntryTime <= endDate.Value.AddDays(1));
            }

            var data = await transactions
                .OrderByDescending(t => t.EntryTime)
                .Select(t => new 
                {
                    TransactionNumber = t.TransactionNumber,
                    VehicleNumber = t.Vehicle == null ? "Unknown" : t.Vehicle.VehicleNumber,
                    VehicleType = t.Vehicle == null ? "Unknown" : t.Vehicle.VehicleType,
                    EntryTime = t.EntryTime,
                    ExitTime = t.ExitTime,
                    Duration = t.ExitTime.HasValue ? 
                        $"{(int)(t.ExitTime.Value - t.EntryTime).TotalHours}h {(t.ExitTime.Value - t.EntryTime).Minutes}m" : 
                        "In Progress",
                    Amount = t.TotalAmount,
                    PaymentStatus = t.PaymentStatus,
                    PaymentMethod = t.PaymentMethod
                })
                .ToListAsync();

            // Convert to CSV
            var csv = "Transaction Number,Vehicle Number,Vehicle Type,Entry Time,Exit Time,Duration,Amount,Payment Status,Payment Method\n";
            foreach (var item in data)
            {
                csv += $"{item.TransactionNumber},{item.VehicleNumber},{item.VehicleType},{item.EntryTime},{item.ExitTime},{item.Duration},{item.Amount},{item.PaymentStatus},{item.PaymentMethod}\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"parking_transactions_{DateTime.Now:yyyyMMdd}.csv");
        }
    }
} 