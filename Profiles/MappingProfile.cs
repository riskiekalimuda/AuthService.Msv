using AuthService.Msv.DTOs;
using AuthService.Msv.Models;
using AutoMapper;

namespace AuthService.Msv.Profiles
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<UserDto, MstUser>();
        }
    }
}
