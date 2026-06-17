using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using QuanLyNhaHangAPI.Models;
using QuanLyNhaHangAPI.Services;
using System.Security.Claims;

namespace QuanLyNhaHangAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public AuthController(
            IAuthService authService,
            IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        #region Login thông thường

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            if (!result.IsSuccess)
                return Unauthorized(result);

            return Ok(result);
        }

        #endregion

        #region Register

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        #endregion

        #region Verify Email

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(
            string email,
            string token)
        {
            var result = await _authService.VerifyEmailAsync(email, token);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        #endregion
        // GOOGLE LOGIN

        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            Console.WriteLine(" GOOGLE LOGIN ");

            var redirectUrl = Url.Action(
                nameof(GoogleCallback),
                "Auth",
                null,
                Request.Scheme);

            Console.WriteLine("Redirect Url : " + redirectUrl);

            AuthenticationProperties properties =
                new AuthenticationProperties
                {
                    RedirectUri = redirectUrl
                };

            return Challenge(
                properties,
                GoogleDefaults.AuthenticationScheme);
        }

        // GOOGLE CALLBACK

        [HttpGet("google-callback")]
public async Task<IActionResult> GoogleCallback()
{
    // Lấy thông tin từ Google
    var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

    // 🔴 SỬA: Đổi BlazorUrl thành AngularUrl khi lỗi
    if (!result.Succeeded)
        return Redirect($"{_configuration["AppSettings:AngularUrl"]}/login?error=google_failed");

    var email = result.Principal?.FindFirstValue(ClaimTypes.Email);
    var hoTen = result.Principal?.FindFirstValue(ClaimTypes.Name);

    if (string.IsNullOrEmpty(email))
        return Redirect($"{_configuration["AppSettings:AngularUrl"]}/login?error=no_email");

    // Gọi service xử lý tạo/lấy user và tạo JWT
    var authResult = await _authService.GoogleLoginAsync(email, hoTen ?? email);

    if (!authResult.IsSuccess)
        return Redirect($"{_configuration["AppSettings:AngularUrl"]}/login?error=login_failed");

    // Encode token để truyền qua URL an toàn
    var encodedToken = Uri.EscapeDataString(authResult.Token!);
    var encodedName = Uri.EscapeDataString(authResult.FullName ?? "");
    var encodedRole = Uri.EscapeDataString(authResult.Role ?? "Khách hàng");

    // 🔴 SỬA: Redirect về Angular kèm JWT token thay vì Blazor
    return Redirect($"{_configuration["AppSettings:AngularUrl"]}/auth/google-result?token={encodedToken}&name={encodedName}&role={encodedRole}");
}

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var result = await _authService.ForgotPasswordAsync(request.Email);
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var result = await _authService.ResetPasswordAsync(request);
            if (!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

    }
}