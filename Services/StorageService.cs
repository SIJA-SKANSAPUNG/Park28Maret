using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace ParkIRC.Services
{
    public interface IStorageService
    {
        Task<string> SaveImageAsync(string base64Image, string folder);
        Task<bool> DeleteImageAsync(string imagePath);
        Task<string> GetImagePathAsync(string fileName, string folder);
        Task<long> GetStorageUsageAsync();
    }

    public class StorageService : IStorageService
    {
        private readonly ILogger<StorageService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _imageStoragePath;

        public StorageService(ILogger<StorageService> logger, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _imageStoragePath = _configuration.GetValue<string>("Storage:ImagePath", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uploads", "images"));
            
            if (!Directory.Exists(_imageStoragePath))
            {
                Directory.CreateDirectory(_imageStoragePath);
            }
        }

        public async Task<string> SaveImageAsync(string base64Image, string folder)
        {
            try
            {
                if (string.IsNullOrEmpty(base64Image) || !base64Image.Contains(","))
                {
                    _logger.LogWarning("Invalid base64 image format");
                    return string.Empty;
                }

                if (string.IsNullOrEmpty(folder))
                {
                    _logger.LogWarning("Folder name cannot be empty");
                    return string.Empty;
                }

                var base64Data = base64Image.Split(',')[1];
                var bytes = Convert.FromBase64String(base64Data);
                var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}.jpg";
                var folderPath = Path.Combine(_imageStoragePath, folder);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, fileName);
                await File.WriteAllBytesAsync(filePath, bytes);

                return Path.Combine("uploads", "images", folder, fileName).Replace("\\", "/");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save image: {Message}", ex.Message);
                return string.Empty;
            }
        }

        public async Task<bool> DeleteImageAsync(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                {
                    return false;
                }

                var relativePath = imagePath.TrimStart('/');
                if (!relativePath.StartsWith("uploads/images/", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Invalid image path format");
                    return false;
                }

                var fullPath = Path.Combine(_imageStoragePath, relativePath.Substring("uploads/images/".Length));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete image: {Message}", ex.Message);
                return false;
            }
        }

        public Task<string> GetImagePathAsync(string fileName, string folder)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(folder))
                {
                    return Task.FromResult(string.Empty);
                }

                var relativePath = Path.Combine("uploads", "images", folder, fileName).Replace("\\", "/");
                var fullPath = Path.Combine(_imageStoragePath, folder, fileName);

                return File.Exists(fullPath) 
                    ? Task.FromResult($"/{relativePath}")
                    : Task.FromResult(string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get image path: {Message}", ex.Message);
                return Task.FromResult(string.Empty);
            }
        }

        public async Task<long> GetStorageUsageAsync()
        {
            try
            {
                if (!Directory.Exists(_imageStoragePath))
                {
                    return 0;
                }

                var directoryInfo = new DirectoryInfo(_imageStoragePath);
                return await Task.Run(() => GetDirectorySize(directoryInfo));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get storage usage: {Message}", ex.Message);
                return 0;
            }
        }

        private long GetDirectorySize(DirectoryInfo directoryInfo)
        {
            long size = 0;
            var files = directoryInfo.GetFiles();
            foreach (var file in files)
            {
                size += file.Length;
            }

            var subDirectories = directoryInfo.GetDirectories();
            foreach (var subDirectory in subDirectories)
            {
                size += GetDirectorySize(subDirectory);
            }

            return size;
        }
    }
}