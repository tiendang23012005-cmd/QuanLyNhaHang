using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace QuanLyNhaHangAPI.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly string _folder;

        // Các định dạng ảnh được phép upload
        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        // Kích thước tối đa cho phép: 5MB
        private const long MaxFileSize = 5 * 1024 * 1024;

        public CloudinaryService(IConfiguration configuration)
        {
            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];
            _folder = configuration["Cloudinary:Folder"] ?? "quanlynhahang";

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<(string Url, string PublicId)> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File ảnh không hợp lệ");
            }

            if (file.Length > MaxFileSize)
            {
                throw new ArgumentException("Kích thước ảnh không được vượt quá 5MB");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new ArgumentException(
                    "Chỉ cho phép tải lên ảnh có định dạng: " +
                    string.Join(", ", AllowedExtensions));
            }

            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = _folder,
                // Tự sinh tên file duy nhất để tránh trùng lặp
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new Exception($"Lỗi upload ảnh lên Cloudinary: {uploadResult.Error.Message}");
            }

            return (uploadResult.SecureUrl.ToString(), uploadResult.PublicId);
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                return false;
            }

            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);

            return result.Result == "ok";
        }
    }
}