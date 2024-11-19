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
            CreateMap<GetUserEntityRequest, User>();
            CreateMap<RegisterUserRequest, User>();
            CreateMap<RegisterUserWithRoleRequest, User>();
            CreateMap<UpdateUserRequest, User>();

            CreateMap<User, GetUserEntityRequest>();
            CreateMap<User, RegisterUserRequest>();
            CreateMap<User, RegisterUserWithRoleRequest>();
            CreateMap<User, UpdateUserRequest>();


            CreateMap<ServerData, Server>();
            CreateMap<ServerRegistrationData, Server>();
            CreateMap<ServerUpdateData, Server>();

            CreateMap<Server, ServerData>();
            CreateMap<Server, ServerRegistrationData>();
            CreateMap<Server, ServerUpdateData>();

            CreateMap<UserProfile, GetProfileRequest>();
            CreateMap<GetProfileRequest, UserProfile>();

            CreateMap<UserFavoritesList, GetFavoritesRequest>();
            CreateMap<GetFavoritesRequest, UserFavoritesList>();

            CreateMap<UserFavoritesListItem, GetFavoriteItemRequest>();
            CreateMap<GetFavoriteItemRequest, UserFavoritesListItem>();
        }
    }
}
