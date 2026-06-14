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
            Console.WriteLine("===== GOOGLE LOGIN =====");

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
            Console.WriteLine("===== GOOGLE CALLBACK =====");

            var authenticateResult =
                await HttpContext.AuthenticateAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

            Console.WriteLine(
                $"Authenticate Success : {authenticateResult.Succeeded}");

            if (!authenticateResult.Succeeded)
            {
                Console.WriteLine("Không lấy được Cookie.");

                return Redirect(
                    $"{_configuration["AppSettings:BlazorUrl"]}/login?error=google_failed");
            }

            ClaimsPrincipal? principal = authenticateResult.Principal;

            if (principal == null)
            {
                return Redirect(
                    $"{_configuration["AppSettings:BlazorUrl"]}/login?error=no_principal");
            }

            Console.WriteLine("===== CLAIM =====");

            foreach (var claim in principal.Claims)
            {
                Console.WriteLine($"{claim.Type} = {claim.Value}");
            }

            string? email =
                principal.FindFirstValue(ClaimTypes.Email);

            string? fullName =
                principal.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("Email NULL");

                return Redirect(
                    $"{_configuration["AppSettings:BlazorUrl"]}/login?error=no_email");
            }


            var loginResult =
                await _authService.GoogleLoginAsync(
                    email,
                    fullName ?? email);

            if (!loginResult.IsSuccess)
            {
                Console.WriteLine(loginResult.Message);

                return Redirect(
                    $"{_configuration["AppSettings:BlazorUrl"]}/login?error=login_failed");
            }


            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            string token =
                Uri.EscapeDataString(loginResult.Token!);

            string name =
                Uri.EscapeDataString(loginResult.FullName ?? "");

            string role =
                Uri.EscapeDataString(loginResult.Role ?? "");

            string url =
                $"{_configuration["AppSettings:BlazorUrl"]}" +
                $"/auth/google-result" +
                $"?token={token}" +
                $"&name={name}" +
                $"&role={role}";

            Console.WriteLine(url);

            return Redirect(url);
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