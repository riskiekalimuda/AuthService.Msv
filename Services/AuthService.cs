using AuthService.Msv.DTOs;
using AuthService.Msv.Models;
using AutoMapper;
using MessageMQCommon.Respones;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Msv.Services
{
    public class AuthService
    {
        private readonly AuthMsvDbContext _dbContext;
        private readonly IMapper _mapper;
        public AuthService(AuthMsvDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
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
    }
}
