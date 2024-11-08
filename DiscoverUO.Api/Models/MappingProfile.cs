using AutoMapper;
using DiscoverUO.Lib.DTOs.Servers;
using DiscoverUO.Lib.DTOs.Users;

namespace DiscoverUO.Api.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserDto, User>();
            CreateMap<CreateUserDto, User>();
            CreateMap<CreateUserWithRoleDto, User>();
            CreateMap<UpdateUserDto, User>();

            CreateMap<User, UserDto>();
            CreateMap<User, CreateUserDto>();
            CreateMap<User, CreateUserWithRoleDto>();
            CreateMap<User, UpdateUserDto>();


            CreateMap<ServerDto, Server>();
            CreateMap<Server, ServerDto>();
        }
    }
}
