namespace QuanLyNhaHangAPI.Services
{
    /// <summary>
    /// Dịch vụ upload / xóa hình ảnh trên Cloudinary.
    /// </summary>
    public interface ICloudinaryService
    {
        /// <summary>
        /// Upload 1 file ảnh lên Cloudinary.
        /// </summary>
        /// <param name="file">File ảnh được gửi lên từ client (multipart/form-data)</param>
        /// <returns>Đường dẫn (URL) công khai của ảnh và public id trên Cloudinary</returns>
        Task<(string Url, string PublicId)> UploadImageAsync(IFormFile file);

        /// <summary>
        /// Xóa 1 ảnh trên Cloudinary dựa theo public id.
        /// </summary>
        Task<bool> DeleteImageAsync(string publicId);
    }
}