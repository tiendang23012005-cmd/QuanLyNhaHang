using QuanLyNhaHangAPI.Models;

namespace QuanLyNhaHangAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> VerifyEmailAsync(string email, string token);
        Task<AuthResponse> LoginAsync(LoginRequest request);

        Task<AuthResponse> GoogleLoginAsync(string email, string hoTen);

        Task<AuthResponse> ForgotPasswordAsync(string email);
        Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request);
    }
}