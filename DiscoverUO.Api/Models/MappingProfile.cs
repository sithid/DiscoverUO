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
            CreateMap<UserUpdateDto, User>();

            CreateMap<User, UserDto>();
            CreateMap<User, CreateUserDto>();
            CreateMap<User, CreateUserWithRoleDto>();
            CreateMap<User, UserUpdateDto>();


            CreateMap<ServerDto, Server>();
            CreateMap<ServerUpdateDto, Server>();

            CreateMap<Server, ServerDto>();
            CreateMap<Server, ServerUpdateDto>();
        }
    }
}
