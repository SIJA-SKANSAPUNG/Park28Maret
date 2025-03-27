using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Logging;
using ParkIRC.Models;
using ParkIRC.Data;
using Microsoft.EntityFrameworkCore;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;
using System.Drawing.Imaging;

namespace ParkIRC.Services
{
    public interface ICameraService
    {
        Task<bool> InitializeAsync();
        Task<string> CaptureImageAsync();
        Task<CameraSettings> GetSettingsAsync();
        Task<bool> UpdateSettingsAsync(CameraSettings settings);
        Task<bool> TestConnectionAsync();
    }

    public class CameraService : ICameraService, IDisposable
    {
        private readonly ILogger<CameraService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private bool _isInitialized;
        private VideoCapture _videoCapture;
        private string _cameraType;
        private CameraSettings _currentSettings;
        private readonly string _imageSavePath;

        public CameraService(ILogger<CameraService> logger, IConfiguration configuration, ApplicationDbContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
            _isInitialized = false;
            _cameraType = configuration.GetValue<string>("Camera:Type", "Webcam");
            _imageSavePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "camera");
            Directory.CreateDirectory(_imageSavePath);
        }



        public bool IsConnected()
        {
            return _isInitialized;
        }

        public void Dispose()
        {
            if (_videoCapture != null)
            {        
                _videoCapture.Dispose();
                _videoCapture = null;
            }
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing camera service...");
                _currentSettings = await GetSettingsAsync();

                if (_cameraType.Equals("Webcam", StringComparison.OrdinalIgnoreCase))
                {
                    int deviceIndex = _configuration.GetValue<int>("Camera:DeviceIndex", 0);
                    _videoCapture = new VideoCapture(deviceIndex);
                    
                    if (!_videoCapture.IsOpened)
                    {
                        throw new InvalidOperationException("No video devices found");
                    }

                    _videoCapture.Set(CapProp.FrameWidth, _currentSettings.ResolutionWidth);
                    _videoCapture.Set(CapProp.FrameHeight, _currentSettings.ResolutionHeight);
                }

                _isInitialized = true;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize camera service");
                return false;
            }
        }

        public async Task<string> CaptureImageAsync()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Camera service is not initialized");
            }

            try
            {
                _logger.LogInformation("Capturing image...");
                
                if (_cameraType.Equals("Webcam", StringComparison.OrdinalIgnoreCase))
                {
                    using var frame = _videoCapture.QueryFrame();
                    if (frame == null)
                    {
                        throw new InvalidOperationException("Failed to capture frame from camera");
                    }

                    string fileName = $"capture_{DateTime.Now:yyyyMMddHHmmss}.jpg";
                    string filePath = Path.Combine(_imageSavePath, fileName);
                    
                    frame.Save(filePath);
                    return filePath;
                }
                else
                {
                    throw new NotImplementedException("IP camera capture not implemented");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to capture image");
                throw;
            }
        }

        public async Task<CameraSettings> GetSettingsAsync()
        {
            try
            {
                var settings = await _context.CameraSettings.FirstOrDefaultAsync();
                return settings ?? new CameraSettings();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get camera settings");
                throw;
            }
        }

        public async Task<bool> UpdateSettingsAsync(CameraSettings settings)
        {
            try
            {
                _context.CameraSettings.Update(settings);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update camera settings");
                return false;
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                _logger.LogInformation("Testing camera connection...");
                
                if (_cameraType.Equals("Webcam", StringComparison.OrdinalIgnoreCase))
                {
                    int deviceIndex = _configuration.GetValue<int>("Camera:DeviceIndex", 0);
                    using var testCapture = new VideoCapture(deviceIndex);
                    
                    if (!testCapture.IsOpened)
                    {
                        return false;
                    }

                    using var frame = testCapture.QueryFrame();
                    return frame != null;
                }
                else
                {
                    throw new NotImplementedException("IP camera test not implemented");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to test camera connection");
                return false;
            }
        }
    }
}