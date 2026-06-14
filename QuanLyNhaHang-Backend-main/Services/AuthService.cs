using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuanLyNhaHangAPI.Data;
using QuanLyNhaHangAPI.Data.Entities;
using QuanLyNhaHangAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace QuanLyNhaHangAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly QuanLyNhaHangDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(QuanLyNhaHangDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // 1. NGHIỆP VỤ ĐĂNG KÝ TÀI KHOẢN (REGISTER) - ĐÃ CHUYỂN THÀNH ASYNC/AWAIT CHUẨN
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                // Kiểm tra trùng lặp Email (Sử dụng AnyAsync bất đồng bộ)
                if (await _context.NguoiDung.AnyAsync(u => u.Email == request.Email))
                {
                    return new AuthResponse { IsSuccess = false, Message = "Email này đã được sử dụng!" };
                }

                // Kiểm tra trùng lặp Số điện thoại 
                if (await _context.NguoiDung.AnyAsync(u => u.DienThoai == request.DienThoai))
                {
                    return new AuthResponse { IsSuccess = false, Message = "Số điện thoại này đã được sử dụng!" };
                }

                // Mã hóa mật khẩu bằng BCrypt
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.MatKhau);

                // Tìm mã vai trò mặc định cho Khách hàng (MaVaiTro = 5)
                var vaiTroKhach = await _context.VaiTro.FirstOrDefaultAsync(r => r.TenVaiTro == "Khách hàng");
                int maVaiTro = vaiTroKhach?.MaVaiTro ?? 5;

                var nguoiDungMoi = new NguoiDung
                {
                    MaVaiTro = maVaiTro,
                    HoTen = request.HoTen,
                    Email = request.Email,
                    DienThoai = request.DienThoai,
                    MatKhau = passwordHash,

                    // Mẹo test Local: Cho bằng true luôn để đăng ký xong đăng nhập được liền, 
                    // khi nào cấu hình xong Email thật thì đổi lại thành false nhé!
                    TrangThaiHoatDong = true,

                    NgayTao = DateTime.Now
                };

                // Lưu dữ liệu bất đồng bộ bảo vệ luồng API không bị treo sập
                await _context.NguoiDung.AddAsync(nguoiDungMoi);
                await _context.SaveChangesAsync();

                // Tạo mã Token kích hoạt gửi kèm (Chạy ngầm không ảnh hưởng luồng chính)
                string activationToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(request.Email));
                SendVerificationEmail(request.Email, request.HoTen, activationToken);

                return new AuthResponse
                {
                    IsSuccess = true,
                    Message = "Đăng ký thành công! Bạn có thể tiến hành đăng nhập ngay lập tức."
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("===== ERROR =====");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("=================");

                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = ex.InnerException?.Message ?? ex.Message
                };
            }
        }

        // 2. NGHIỆP VỤ XÁC THỰC EMAIL (VERIFY EMAIL)
        public async Task<AuthResponse> VerifyEmailAsync(string email, string token)
        {
            try
            {
                var user = await _context.NguoiDung.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                    return new AuthResponse { IsSuccess = false, Message = "Tài khoản không tồn tại." };

                string expectedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(email));
                if (token != expectedToken)
                    return new AuthResponse { IsSuccess = false, Message = "Mã xác thực không hợp lệ." };

                user.TrangThaiHoatDong = true;
                await _context.SaveChangesAsync();

                return new AuthResponse { IsSuccess = true, Message = "Xác thực tài khoản thành công! Bạn có thể đăng nhập." };
            }
            catch (Exception ex)
            {
                return new AuthResponse { IsSuccess = false, Message = $"Lỗi xác thực: {ex.Message}" };
            }
        }

        // 3. NGHIỆP VỤ ĐĂNG NHẬP TRẢ VỀ JWT (LOGIN)
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                // Đăng nhập linh hoạt bằng Email hoặc Số điện thoại
                var user = await _context.NguoiDung
                    .Include(u => u.MaVaiTroNavigation)
                    .FirstOrDefaultAsync(u => u.Email == request.Identifer || u.DienThoai == request.Identifer || u.TenDangNhap == request.Identifer);

                if (user == null || !BCrypt.Net.BCrypt.Verify(request.MatKhau, user.MatKhau))
                    return new AuthResponse { IsSuccess = false, Message = "Tài khoản hoặc mật khẩu không chính xác!" };

                if (user.TrangThaiHoatDong == false)
                    return new AuthResponse { IsSuccess = false, Message = "Tài khoản của bạn chưa được kích hoạt hoặc đã bị khóa!" };

                // Sinh Token JWT chứa thông tin quyền hạn
                string token = GenerateJwtToken(user);

                return new AuthResponse
                {
                    IsSuccess = true,
                    Message = "Đăng nhập thành công!",
                    Token = token,
                    FullName = user.HoTen,
                    Role = user.MaVaiTroNavigation?.TenVaiTro ?? "Khách hàng"
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse { IsSuccess = false, Message = $"Lỗi xử lý đăng nhập: {ex.Message}" };
            }
        }

        // PHƯƠNG THỨC PHỤ 1: KHỞI TẠO JWT TOKEN CHUẨN
        private string GenerateJwtToken(NguoiDung user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.MaNguoiDung.ToString()),
                new Claim(ClaimTypes.Name, user.HoTen),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.MaVaiTroNavigation?.TenVaiTro ?? "Khách hàng")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "Key_Bao_Mat_Sieu_Cap_Nha_Hang_Quan_Ly_123456789"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds,
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var createdToken = tokenHandler.CreateToken(token);

            return tokenHandler.WriteToken(createdToken);
        }

        // PHƯƠNG THỨC PHỤ 2: GỬI MAIL KÍCH HOẠT SỬ DỤNG SMTP CƠ BẢN
        private void SendVerificationEmail(string toEmail, string fullName, string token)
        {
            try
            {
                string callbackUrl = $"{_config["AppSettings:AngularUrl"]}/verify-email?email={toEmail}&token={token}";

                var fromAddress = new MailAddress("hethongnhahang@gmail.com", "Nhà Hàng Quản Lý");
                var toAddress = new MailAddress(toEmail, fullName);

                using (var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("YOUR_EMAIL@gmail.com", "YOUR_APP_PASSWORD"),
                    Timeout = 5000 // Tự động ngắt kết nối sau 5 giây nếu cấu hình giả lập để tránh treo luồng dự án
                })
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = "[Hệ Thống Nhà Hàng] - Kích Hoạt Tài Khoản Đăng Ký",
                    Body = $"<h3>Chào {fullName},</h3><p>Vui lòng click vào đường link dưới đây để hoàn tất kích hoạt tài khoản của bạn trên hệ thống:</p><a href='{callbackUrl}'>KÍCH HOẠT NGAY TÀI KHOẢN</a>",
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception)
            {
                // Bắt toàn bộ lỗi Mail giả lập, cho phép luồng ghi DB chính được thành công mượt màaaa
            }
        }

        // ✅ THÊM METHOD NÀY
        public async Task<AuthResponse> GoogleLoginAsync(string email, string hoTen)
        {
            try
            {
                // Tìm user theo email Google
                var user = await _context.NguoiDung
                    .Include(u => u.MaVaiTroNavigation)
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    // Tự động tạo tài khoản mới nếu chưa có
                    var vaiTroKhach = await _context.VaiTro.FirstOrDefaultAsync(r => r.TenVaiTro == "Khách hàng");
                    int maVaiTro = vaiTroKhach?.MaVaiTro ?? 5;

                    user = new NguoiDung
                    {
                        MaVaiTro = maVaiTro,
                        HoTen = hoTen,
                        Email = email,
                        MatKhau = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), // Mật khẩu ngẫu nhiên
                        TrangThaiHoatDong = true,
                        NgayTao = DateTime.Now
                    };

                    await _context.NguoiDung.AddAsync(user);
                    await _context.SaveChangesAsync();

                    // Load lại navigation property
                    user = await _context.NguoiDung
                        .Include(u => u.MaVaiTroNavigation)
                        .FirstAsync(u => u.Email == email);
                }

                if (user.TrangThaiHoatDong == false)
                    return new AuthResponse { IsSuccess = false, Message = "Tài khoản đã bị khóa!" };

                string token = GenerateJwtToken(user);

                return new AuthResponse
                {
                    IsSuccess = true,
                    Message = "Đăng nhập Google thành công!",
                    Token = token,
                    FullName = user.HoTen,
                    Role = user.MaVaiTroNavigation?.TenVaiTro ?? "Khách hàng"
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse { IsSuccess = false, Message = $"Lỗi đăng nhập Google: {ex.Message}" };
            }
        }

        // ✅ THÊM METHOD 1: Gửi email đặt lại mật khẩu
        public async Task<AuthResponse> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await _context.NguoiDung
                    .FirstOrDefaultAsync(u => u.Email == email);

                // Luôn trả về thành công để tránh lộ thông tin email tồn tại hay không
                if (user == null)
                    return new AuthResponse { IsSuccess = true, Message = "Nếu email tồn tại, bạn sẽ nhận được link đặt lại mật khẩu." };

                // Tạo token reset (kết hợp email + timestamp để có hạn dùng)
                string rawToken = $"{email}|{DateTime.UtcNow.AddMinutes(30):yyyy-MM-ddTHH:mm:ss}";
                string resetToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(rawToken));

                // Gửi email
                string blazorUrl = _config["AppSettings:BlazorUrl"] ?? "https://localhost:7144";
                string resetLink = $"{blazorUrl}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(resetToken)}";

                SendResetPasswordEmail(email, user.HoTen, resetLink);

                return new AuthResponse { IsSuccess = true, Message = "Nếu email tồn tại, bạn sẽ nhận được link đặt lại mật khẩu." };
            }
            catch (Exception ex)
            {
                return new AuthResponse { IsSuccess = false, Message = $"Lỗi hệ thống: {ex.Message}" };
            }
        }

        // ✅ THÊM METHOD 2: Đặt lại mật khẩu mới
        public async Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                // Giải mã token
                string rawToken = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(request.Token));
                var parts = rawToken.Split('|');
                if (parts.Length != 2)
                    return new AuthResponse { IsSuccess = false, Message = "Link không hợp lệ." };

                string tokenEmail = parts[0];
                if (!DateTime.TryParse(parts[1], out DateTime expiry))
                    return new AuthResponse { IsSuccess = false, Message = "Link không hợp lệ." };

                // Kiểm tra email khớp
                if (tokenEmail != request.Email)
                    return new AuthResponse { IsSuccess = false, Message = "Link không hợp lệ." };

                // Kiểm tra hết hạn (30 phút)
                if (DateTime.UtcNow > expiry)
                    return new AuthResponse { IsSuccess = false, Message = "Link đã hết hạn. Vui lòng yêu cầu lại." };

                // Tìm user và cập nhật mật khẩu
                var user = await _context.NguoiDung.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null)
                    return new AuthResponse { IsSuccess = false, Message = "Tài khoản không tồn tại." };

                user.MatKhau = BCrypt.Net.BCrypt.HashPassword(request.MatKhauMoi);
                await _context.SaveChangesAsync();

                return new AuthResponse { IsSuccess = true, Message = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập." };
            }
            catch (Exception ex)
            {
                return new AuthResponse { IsSuccess = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        // ✅ THÊM METHOD PHỤ: Gửi email reset
        private void SendResetPasswordEmail(string toEmail, string fullName, string resetLink)
        {
            try
            {
                var emailSettings = _config.GetSection("EmailSettings");
                var fromAddress = new System.Net.Mail.MailAddress(
                    emailSettings["FromEmail"]!, emailSettings["FromName"]);
                var toAddress = new System.Net.Mail.MailAddress(toEmail, fullName);

                using var smtp = new System.Net.Mail.SmtpClient
                {
                    Host = emailSettings["SmtpHost"]!,
                    Port = int.Parse(emailSettings["SmtpPort"]!),
                    EnableSsl = true,
                    DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(
                        emailSettings["SmtpUser"],
                        emailSettings["SmtpPassword"]),
                    Timeout = 10000
                };

                using var message = new System.Net.Mail.MailMessage(fromAddress, toAddress)
                {
                    Subject = "[Nhà Hàng] - Đặt lại mật khẩu",
                    Body = $@"
                <div style='font-family:Arial,sans-serif;max-width:500px;margin:auto'>
                    <h2 style='color:#f59e0b'>🍽️ Quản Lý Nhà Hàng</h2>
                    <p>Xin chào <strong>{fullName}</strong>,</p>
                    <p>Bạn vừa yêu cầu đặt lại mật khẩu. Nhấn vào nút bên dưới để tiếp tục:</p>
                    <div style='text-align:center;margin:30px 0'>
                        <a href='{resetLink}'
                           style='background:#f59e0b;color:white;padding:12px 30px;border-radius:6px;text-decoration:none;font-weight:bold'>
                            ĐẶT LẠI MẬT KHẨU
                        </a>
                    </div>
                    <p style='color:#6b7280;font-size:13px'>Link có hiệu lực trong <strong>30 phút</strong>. Nếu bạn không yêu cầu, hãy bỏ qua email này.</p>
                </div>",
                    IsBodyHtml = true
                };

                smtp.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gửi email: {ex.Message}");
            }
        }

    }
}