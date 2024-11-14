using AutoMapper;
using DiscoverUO.Lib.DTOs.Favorites;
using DiscoverUO.Lib.DTOs.Profiles;
using DiscoverUO.Lib.DTOs.Servers;
using DiscoverUO.Lib.DTOs.Users;

namespace DiscoverUO.Api.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserRequest, User>();
            CreateMap<RegisterUserRequest, User>();
            CreateMap<RegisterUserWithRoleRequest, User>();
            CreateMap<UpdateUserRequestRequest, User>();

            CreateMap<User, UserRequest>();
            CreateMap<User, RegisterUserRequest>();
            CreateMap<User, RegisterUserWithRoleRequest>();
            CreateMap<User, UpdateUserRequestRequest>();


            CreateMap<ServerDto, Server>();
            CreateMap<CreateServerDto, Server>();
            CreateMap<UpdateServerDto, Server>();

            CreateMap<Server, ServerDto>();
            CreateMap<Server, CreateServerDto>();
            CreateMap<Server, UpdateServerDto>();

            CreateMap<UserProfile, UserProfileDto>();
            CreateMap<UserProfileDto, UserProfile>();

            CreateMap<UserFavoritesList, UserFavoritesListDto>();
            CreateMap<UserFavoritesListDto, UserFavoritesList>();

            CreateMap<UserFavoritesListItem, UserFavoritesListItemDto>();
            CreateMap<UserFavoritesListItemDto, UserFavoritesListItem>();
        }
    }
}
