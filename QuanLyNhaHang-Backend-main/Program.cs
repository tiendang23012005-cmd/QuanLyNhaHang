using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuanLyNhaHangAPI.Data;
using QuanLyNhaHangAPI.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

namespace QuanLyNhaHangAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connStr = builder.Configuration.GetConnectionString("DefaultConnection");

            Console.WriteLine("=================================");
            Console.WriteLine(connStr);
            Console.WriteLine("=================================");

            // ✅ THÊM trước builder.Build()
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            // 1. Cấu hình kết nối SQL Server Database
            builder.Services.AddDbContext<QuanLyNhaHangDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 2. Kích hoạt Dependency Injection cho AuthService
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<VNPayService>();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //================ JWT + GOOGLE ===================

            var jwtKey = builder.Configuration["Jwt:Key"]!;

            builder.Services
            .AddAuthentication(options =>
            {
                // API sử dụng JWT
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                // Google sẽ lưu thông tin đăng nhập tạm bằng Cookie
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })

            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = "GoogleAuth";

                options.Cookie.HttpOnly = true;

                options.Cookie.SameSite = SameSiteMode.Lax;

                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
            })

            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = builder.Configuration["Jwt:Issuer"],

                    ValidAudience = builder.Configuration["Jwt:Audience"],

                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            })

            .AddGoogle(options =>
            {
                options.ClientId =
                    builder.Configuration["Google:ClientId"];

                options.ClientSecret =
                    builder.Configuration["Google:ClientSecret"];

                options.CallbackPath = "/signin-google";

                options.SignInScheme =
                    CookieAuthenticationDefaults.AuthenticationScheme;

                options.SaveTokens = true;

                options.Scope.Add("email");
                options.Scope.Add("profile");
            });

            // =========================================================================
            // SỬA TẠI ĐÂY: Gộp chung các domain Frontend vào 1 chính sách CORS duy nhất
            // =========================================================================
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontends", policy =>
                {
                    policy.WithOrigins(
                              "http://localhost:4200",  // Cổng chạy Angular mặc định
                              "https://localhost:7144", // Cổng HTTPS của Blazor WASM
                              "http://localhost:5000"   // Cổng HTTP của Blazor WASM
                          )
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials(); // Bắt buộc nếu bạn dùng Cookie hoặc xác thực nâng cao qua CORS
                });
            });

            builder.Services.AddScoped<INhanVienService, NhanVienService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // =========================================================================
            // SỬA TẠI ĐÂY: Chỉ gọi UseCors MỘT LẦN duy nhất với chính sách chung
            // =========================================================================        
            app.UseHttpsRedirection();

            app.UseCors("AllowFrontends");

            app.UseSession();

            // Lưu ý: Thứ tự chuẩn là UseCors -> UseAuthentication -> UseAuthorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Đoạn code Hash lại mật khẩu tự động của bạn giữ nguyên
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<QuanLyNhaHangDbContext>();
                var users = context.NguoiDung.ToList();
                foreach (var user in users)
                {
                    if (!user.MatKhau.StartsWith("$2"))
                    {
                        user.MatKhau = BCrypt.Net.BCrypt.HashPassword(user.MatKhau);
                    }
                }
                context.SaveChanges();
            }

            app.Run();
        }
    }
}