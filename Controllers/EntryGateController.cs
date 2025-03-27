using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkIRC.Data;
using ParkIRC.Models;
using ParkIRC.Models.ViewModels;
using ParkIRC.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ParkIRC.Controllers
{
    [Authorize(Roles = "Admin,Operator")]
    public class EntryGateController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EntryGateController> _logger;
        private readonly ConnectionStatusService _connectionStatusService;
        private readonly CameraService _cameraService;
        private readonly StorageService _storageService;
        private readonly TicketService _ticketService;
        private readonly PrinterService _printerService;

        public EntryGateController(
            ApplicationDbContext context,
            ILogger<EntryGateController> logger,
            ConnectionStatusService connectionStatusService,
            CameraService cameraService,
            StorageService storageService,
            TicketService ticketService,
            PrinterService printerService)
        {
            _context = context;
            _logger = logger;
            _connectionStatusService = connectionStatusService;
            _cameraService = cameraService;
            _storageService = storageService;
            _ticketService = ticketService;
            _printerService = printerService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new EntryGateMonitoringViewModel();
            
            // Get entry gates statuses
            model.EntryGates = await _context.EntryGates
                .Where(g => g.IsActive)
                .Select(g => new EntryGateStatusViewModel
                {
                    Id = g.Id,
                    Name = g.Name,
                    IsOnline = g.IsOnline,
                    IsOpen = g.IsOpen,
                    LastActivity = g.LastActivity,
                    VehiclesProcessed = _context.ParkingTransactions
                        .Count(t => t.EntryPoint == g.Id.ToString() && t.EntryTime.Date == DateTime.Today)
                })
                .ToListAsync();
            
            // Get recent transactions
            model.RecentTransactions = await _context.ParkingTransactions
                .Include(t => t.Vehicle)
                .Where(t => t.EntryTime > DateTime.Now.AddHours(-1))
                .OrderByDescending(t => t.EntryTime)
                .Take(10)
                .Select(t => new RecentTransactionViewModel
                {
                    Id = t.Id,
                    TicketNumber = int.TryParse(t.TicketNumber, out int ticketNum) ? ticketNum : 0,
                    VehicleNumber = t.Vehicle.VehicleNumber,
                    VehicleType = t.Vehicle.VehicleType,
                    EntryTime = t.EntryTime,
                    EntryPoint = int.TryParse(t.EntryPoint, out int entryPoint) ? entryPoint : 0
                })
                .ToListAsync();
            
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddEntryGate([FromBody]EntryGate gate)
        {
            if (ModelState.IsValid)
            {
                int gateCount = await _context.EntryGates.CountAsync() + 1;
                gate.Id = gateCount;
                gate.IsActive = true;
                gate.LastActivity = DateTime.Now;
                
                _context.EntryGates.Add(gate);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"New entry gate added: {gate.Name}");
                
                return Ok(new { 
                    id = gate.Id, 
                    name = gate.Name 
                });
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateEntryGate(string id, bool isOnline)
        {
            var gate = await _context.EntryGates.FindAsync(id);
            if (gate == null)
            {
                return NotFound();
            }

            gate.IsOnline = isOnline;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetGateStatus(string id)
        {
            var gate = await _context.EntryGates.FindAsync(id);
            if (gate == null)
            {
                return NotFound();
            }

            return Json(new { 
                isOnline = gate.IsOnline, 
                isOpen = gate.IsOpen 
            });
        }

        [HttpPost]
        public async Task<IActionResult> CaptureEntry([FromBody] EntryRequest request)
        {
            try {
                // Capture foto kendaraan
                var imagePath = await _cameraService.CaptureImageAsync();
                
                // Read captured image
                var imageBytes = await System.IO.File.ReadAllBytesAsync(imagePath);
                var base64Image = $"data:image/jpeg;base64,{Convert.ToBase64String(imageBytes)}";
                
                // Generate nama file unik
                string fileName = $"{DateTime.Now:yyyyMMddHHmmss}_{request.GateId}.jpg";
                
                // Simpan foto
                var savedImagePath = await _storageService.SaveImageAsync(base64Image, "vehicles");
                
                // Buat transaksi parkir
                var transaction = new ParkingTransaction 
                {
                    TicketNumber = await _ticketService.GenerateTicketNumber(request.VehicleType),
                    EntryTime = DateTime.Now,
                    EntryPoint = request.GateId.ToString(), 
                    VehicleImagePath = savedImagePath,
                    VehicleNumber = request.VehicleNumber, 
                    VehicleType = request.VehicleType,     
                    IsManualEntry = true
                };

                await _context.ParkingTransactions.AddAsync(transaction);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    ticketNumber = transaction.TicketNumber,
                    imagePath = fileName
                });
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error processing entry");
                return StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AssignSpace(VehicleEntryRequest request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(request.VehicleType) || 
                    string.IsNullOrEmpty(request.VehicleNumber))
                {
                    return BadRequest(new { message = "Invalid vehicle information" });
                }

                // Parse GateId to int if needed
                if (!int.TryParse(request.GateId, out int gateId))
                {
                    return BadRequest(new { message = "Invalid gate ID" });
                }

                // Automatic space assignment
                var availableSpace = await _context.ParkingSpaces
                    .Where(s => !s.IsOccupied && s.SpaceType.ToLower() == request.VehicleType.ToLower())
                    .OrderBy(s => s.SpaceNumber)
                    .FirstOrDefaultAsync();

                if (availableSpace == null)
                {
                    return BadRequest(new { message = "Tidak ada slot parkir tersedia" });
                }

                // Generate ticket number
                string ticketNumber = await _ticketService.GenerateTicketNumber(request.VehicleType);

                // Update parking space
                availableSpace.IsOccupied = true;
                availableSpace.LastOccupiedTime = DateTime.Now;

                // Create parking transaction
                var transaction = new ParkingTransaction
                {
                    TicketNumber = ticketNumber,
                    EntryTime = DateTime.Now,
                    EntryPoint = gateId.ToString(), 
                    VehicleNumber = request.VehicleNumber,
                    VehicleType = request.VehicleType,
                    ParkingSpaceId = availableSpace.Id, 
                    IsManualEntry = true
                };

                _context.ParkingTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                return Ok(new 
                { 
                    ticketNumber = ticketNumber, 
                    spaceNumber = availableSpace.SpaceNumber 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning parking space");
                return StatusCode(500, new { message = "Terjadi kesalahan saat menetapkan ruang parkir" });
            }
        }
    }
}