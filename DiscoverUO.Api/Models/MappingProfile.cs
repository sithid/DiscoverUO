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
            CreateMap<UserEntityData, User>();
            CreateMap<RegisterUserData, User>();
            CreateMap<RegisterUserWithRoleData, User>();
            CreateMap<UpdateUserData, User>();

            CreateMap<User, UserEntityData>();
            CreateMap<User, RegisterUserData>();
            CreateMap<User, RegisterUserWithRoleData>();
            CreateMap<User, UpdateUserData>();


            CreateMap<ServerData, Server>();
            CreateMap<ServerRegistrationData, Server>();
            CreateMap<ServerUpdateData, Server>();

            CreateMap<Server, ServerData>();
            CreateMap<Server, ServerRegistrationData>();
            CreateMap<Server, ServerUpdateData>();

            CreateMap<UserProfile, ProfileData>();
            CreateMap<ProfileData, UserProfile>();

            CreateMap<UserFavoritesList, FavoritesData>();
            CreateMap<FavoritesData, UserFavoritesList>();

            CreateMap<UserFavoritesListItem, FavoriteItemData>();
            CreateMap<FavoriteItemData, UserFavoritesListItem>();
        }
    }
}
