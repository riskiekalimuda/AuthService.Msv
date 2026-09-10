// Pengaturan ini memaksa .NET dan Npgsql menyelaraskan format DateTime lama/lokal menjadi kompatibel dengan pemformatan database
using AuthService.Msv.Models;
using AuthService.Msv.Profiles;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Yarp.ReverseProxy.Transforms;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuthMsvDbContext>(options => 
options.UseNpgsql(builder.Configuration.GetConnectionString("AuthMsvDBConnection")));

builder.Services.AddControllers();

builder.Services.AddAutoMapper(x => { }, typeof(MappingProfile));

builder.Services.AddScoped<AuthService.Msv.Services.AuthService>();


// 1. Ambil JWT Key dari Environment Variable (yang sudah kita buat sebelumnya)
var jwtKey = builder.Configuration["Jwt_Key"] ?? builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new Exception("JWT Key tidak ditemukan!");
}

// 2. Tambahkan Layanan Autentikasi JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Kebijakan default untuk rute yang membutuhkan login
    options.AddPolicy("RegisteredUser", policy => policy.RequireAuthenticatedUser());
});

// 3. Konfigurasi YARP dan Mekanisme Penerusan Header (Transforms)
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(transformContext =>
    {
        // Transform ini berjalan setiap kali ada request yang diteruskan ke mikroservis internal
        transformContext.AddRequestTransform(async requestContext =>
        {
            var user = requestContext.HttpContext.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                // Ambil UserId (UUID) dan Role dari token JWT
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var role = user.FindFirst(ClaimTypes.Role)?.Value;

                // Suntikkan ke HTTP Header sebelum dikirim ke mikroservis internal (misal: OrderService)
                if (!string.IsNullOrEmpty(userId))
                    requestContext.ProxyRequest.Headers.Add("X-User-Id", userId);

                if (!string.IsNullOrEmpty(role))
                    requestContext.ProxyRequest.Headers.Add("X-User-Role", role);
            }
            await Task.CompletedTask;
        });
    });

var app = builder.Build();

app.UseRouting();

// Wajib diletakkan sebelum UseAuthorization dan MapReverseProxy
app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();


app.MapControllers();


app.Run();


