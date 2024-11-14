using AutoMapper;
using DiscoverUO.Lib.Shared.Users;
using DiscoverUO.Lib.Shared.Favorites;
using DiscoverUO.Lib.Shared.Profiles;
using DiscoverUO.Lib.Shared.Servers;

namespace DiscoverUO.Api.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<GetUserRequest, User>();
            CreateMap<RegisterUserRequest, User>();
            CreateMap<RegisterUserWithRoleRequest, User>();
            CreateMap<UpdateUserRequestRequest, User>();

            CreateMap<User, GetUserRequest>();
            CreateMap<User, RegisterUserRequest>();
            CreateMap<User, RegisterUserWithRoleRequest>();
            CreateMap<User, UpdateUserRequestRequest>();


            CreateMap<GetServerRequest, Server>();
            CreateMap<RegisterServerRequest, Server>();
            CreateMap<UpdateServerRequest, Server>();

            CreateMap<Server, GetServerRequest>();
            CreateMap<Server, RegisterServerRequest>();
            CreateMap<Server, UpdateServerRequest>();

            CreateMap<UserProfile, GetProfileRequest>();
            CreateMap<GetProfileRequest, UserProfile>();

            CreateMap<UserFavoritesList, GetFavoritesRequest>();
            CreateMap<GetFavoritesRequest, UserFavoritesList>();

            CreateMap<UserFavoritesListItem, GetFavoriteItemRequest>();
            CreateMap<GetFavoriteItemRequest, UserFavoritesListItem>();
        }
    }
}
