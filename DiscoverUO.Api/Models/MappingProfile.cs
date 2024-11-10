using AutoMapper;
using DiscoverUO.Lib.DTOs.Servers;
using DiscoverUO.Lib.DTOs.Users;
using DiscoverUO.Lib.DTOs.Profiles;
using DiscoverUO.Lib.DTOs.Favorites;

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
            CreateMap<CreateServerDto, Server>();
            CreateMap<UpdateServerDto, Server>();

            CreateMap<Server, ServerDto>();
            CreateMap<Server, CreateServerDto>();
            CreateMap<Server, UpdateServerDto>();

            CreateMap<UserProfile, UserProfileDto>();
            CreateMap<UserProfileDto, UserProfile>();

            CreateMap<UserFavoritesListDto, UserFavoritesListDto>();
            CreateMap<UserFavoritesListDto, UserFavoritesListDto>();
        }
    }
}
