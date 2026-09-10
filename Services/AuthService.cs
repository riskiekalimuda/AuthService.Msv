using AuthService.Msv.DTOs;
using AuthService.Msv.Models;
using AutoMapper;
using MessageMQCommon.Respones;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Msv.Services
{
    public class AuthService
    {
        private readonly AuthMsvDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        public AuthService(AuthMsvDbContext dbContext, IMapper mapper, IConfiguration config)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _config = config;   
        }

        public async Task<ServiceResult<string>> Login(UserDto userDto)
        {
            if (userDto == null)
            {
                return new ServiceResult<string>(false) { IsSuccess=false, ErrorCode="PAYLOAD_NULL", ErrorMessage="User DTO is null." };
            }

            var user = await _dbContext.MstUsers.FirstOrDefaultAsync(x=>x.Username == userDto.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(userDto.Password, user.PasswordHash))
            {
                return new ServiceResult<string>(false) { IsSuccess=false, ErrorCode="INVALID_USER_OR_PASSWORD", ErrorMessage= "INVALID_USER_OR_PASSWORD" };
            }
            var token = CreateToken(user);
            if (token == null)
            {
                return new ServiceResult<string>(false) { IsSuccess=false, ErrorCode="UNAVAILABLE_TOKEN", ErrorMessage="Unavailable token."};
            }
            return new ServiceResult<string>(true) { IsSuccess = true, Data = token };
        }

        public async Task<ServiceResult<MstUser>> RegisterUser(UserDto userDto)
        {
            if (userDto == null)
            {
                return new ServiceResult<MstUser>(false) { IsSuccess=false, ErrorCode="NULL PAYLOAD", ErrorMessage="Null payload user." };
            }
            var userExist = await _dbContext.MstUsers.AnyAsync(x=>x.Username == userDto.Username);
            if (userExist)
            {
                return new ServiceResult<MstUser>(false) { IsSuccess=false, ErrorCode="NOT AVAILABLE USER", ErrorMessage="User has been used." };
            }
            var dataUser = _mapper.Map<MstUser>(userDto);
            try
            {
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
                dataUser.PasswordHash = passwordHash;
                dataUser.Role = "Admin";
                await _dbContext.AddAsync(dataUser);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new ServiceResult<MstUser>(false) { IsSuccess =false, ErrorCode="DATABASE_ERROR", ErrorMessage=$"{ex.Message}" }; 
            }

            return new ServiceResult<MstUser>(true)
            {
                IsSuccess = true,
                Data=dataUser
            };
        }
        // === FUNGSI GENERATE TOKEN JWT ===
        private string CreateToken(MstUser user)
        {
            // Menyimpan klaim (informasi user) ke dalam token
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Menggunakan ID UUID acak dari Postgres
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

            // Ambil JWT Key dari Environment Variable (yang bernilai minimal 64 karakter)
            var jwtKey = _config["Jwt_Key"] ?? _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new Exception("JWT Key tidak ditemukan di konfigurasi!");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1), // Token berlaku selama 1 hari
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
