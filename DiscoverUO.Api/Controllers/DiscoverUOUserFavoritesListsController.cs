using AutoMapper;
using DiscoverUO.Api.Models;
using DiscoverUO.Lib.Shared.Data;
using DiscoverUO.Lib.Shared.Contracts;
using DiscoverUO.Lib.Shared.Favorites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace DiscoverUO.Api.Controllers
{
    [Route("api/favorites")]
    [ApiController]
    public class DiscoverUOUserFavoritesListsController : ControllerBase
    {
        #region Private Fields

        private readonly DiscoverUODatabaseContext _context;
        private readonly IMapper _mapper;

        #endregion

        public DiscoverUOUserFavoritesListsController(DiscoverUODatabaseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        #region BasicUser Endpoints

        [Authorize] // IResponse
        [HttpGet("list/view")]
        public async Task<ActionResult<IResponse>> GetUserFavoritesLists()
        {
            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                var failedUnauthorized = new RequestFailedResponse
                {
                    Success = false,
                    Message = "You must be logged in to do this.",
                    StatusCode = HttpStatusCode.Unauthorized
                };

                return Unauthorized(failedUnauthorized);
            }

            var currentUserFavorites = await _context.UserFavoritesLists
                .Include(flist => flist.FavoritedItems)
                .FirstOrDefaultAsync(flist => flist.OwnerId == currentUser.Id);

            if( currentUserFavorites.FavoritedItems == null || currentUserFavorites.FavoritedItems.Count <= 0 )
            {
                var failedNotFound = new RequestFailedResponse
                {
                    Success = false,
                    Message = "Favorited items not found.",
                    StatusCode = HttpStatusCode.NotFound
                };

                return NotFound(failedNotFound);
            }

            var favoritesData = _mapper.Map<FavoritesData>(currentUserFavorites);

            var favoritesDataResponse = new FavoritesDataReponse
            {
                Success = true,
                Message = "Favorites data found.",
                StatusCode = HttpStatusCode.OK,
                Entity = favoritesData
            };

            return Ok(favoritesDataResponse);
        }

        [Authorize] // IResponse
        [HttpGet("list/item/view/{id}")]
        public async Task<ActionResult<IResponse>> GetUserFavoritesListItemById(int id)
        {
            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                var failedUnauthorized = new RequestFailedResponse
                {
                    Success = false,
                    Message = "You must be logged in to do this.",
                    StatusCode = HttpStatusCode.Unauthorized
                };

                return Unauthorized(failedUnauthorized);
            }

            var favoritesListItem = await _context.UserFavoritesListItems
                .FirstOrDefaultAsync(f => f.Id == id);

            if (favoritesListItem == null)
            {
                var failedNotFound = new RequestFailedResponse
                {
                    Success = false,
                    Message = "That favorites list item was not found.",
                    StatusCode = HttpStatusCode.NotFound
                };

                return NotFound(failedNotFound);
            }

            if (currentUser.Id != favoritesListItem.OwnerId)
            {
                if (!Permissions.HasElevatedRole(currentUser.Role))
                {
                    var failedUnauthorized = new RequestFailedResponse
                    {
                        Success = false,
                        Message = "You do not have permission to do that.",
                        StatusCode = HttpStatusCode.Unauthorized
                    };

                    return Unauthorized(failedUnauthorized);
                }
            }

            var favoritesListItemDto = _mapper.Map<FavoriteItemData>(favoritesListItem);

            return Ok(favoritesListItemDto);
        }

        [Authorize] // IResponse
        [HttpPut("list/item/update/{id}")]
        public async Task<ActionResult<IResponse>> UpdateFavoritesListItem(int id, FavoriteItemData userFavoritesListItemData)
        {
            if (!ModelState.IsValid)
            {
                var failedResponse = new RequestFailedResponse
                {
                    Success = false,
                    Message = "Invalid ModelState!",
                    StatusCode = HttpStatusCode.BadRequest
                };

                return BadRequest(failedResponse);
            }

            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                var failedResponse = new RequestFailedResponse
                {
                    Success = false,
                    Message = "You must be logged in to do this.",
                    StatusCode = HttpStatusCode.Unauthorized
                };

                return Unauthorized(failedResponse);
            }

            var userFavoritesListItem = await _context.UserFavoritesListItems
                .FirstOrDefaultAsync(item => item.Id == id);

            if (userFavoritesListItem == null)
            {
                var failedResponse = new RequestFailedResponse
                {
                    Success = false,
                    Message = "The favorites list item was not found.",
                    StatusCode = HttpStatusCode.NotFound
                };

                return NotFound(failedResponse);
            }

            if (userFavoritesListItem.OwnerId != currentUser.Id)
            {
                if (!Permissions.HasElevatedRole(currentUser.Role))
                {
                    var failedResponse = new RequestFailedResponse
                    {
                        Success = false,
                        Message = "You can only edit your own favorites list.",
                        StatusCode = HttpStatusCode.Unauthorized
                    };

                    return Unauthorized(failedResponse);
                }
            }

            userFavoritesListItem.ServerName = userFavoritesListItemData.ServerName;
            userFavoritesListItem.ServerAddress = userFavoritesListItemData.ServerAddress;
            userFavoritesListItem.ServerPort = userFavoritesListItemData.ServerPort;
            userFavoritesListItem.ServerEra = userFavoritesListItemData.ServerEra;
            userFavoritesListItem.PvPEnabled = userFavoritesListItemData.PvPEnabled;
            userFavoritesListItem.ServerWebsite = userFavoritesListItemData.ServerWebsite;
            userFavoritesListItem.ServerBanner = userFavoritesListItemData.ServerBanner;
            userFavoritesListItem.ServerId = userFavoritesListItemData.ServerId;

            _context.Entry(userFavoritesListItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var failedBadRequest = new RequestFailedResponse
                {
                    Success = false,
                    Message = $"Something happened while updating the list item. {ex}",
                    StatusCode = HttpStatusCode.Unauthorized
                };

                return BadRequest(failedBadRequest);
            }

            var updatedUserFavoritesListItem = await _context.UserFavoritesListItems
                .FirstOrDefaultAsync(item => item.Id == id);

            userFavoritesListItemData = _mapper.Map<FavoriteItemData>(updatedUserFavoritesListItem);

            var updatedItemData = new FavoriteItemDataReponse
            {
                Success = true,
                Message = "The favorite item was updated.",
                StatusCode = HttpStatusCode.OK,
                Entity = userFavoritesListItemData
            };
            
            return Ok(updatedItemData);
        }

        [Authorize] // IResponse
        [HttpPost("list/item/add")]
        public async Task<ActionResult> AddFavoritesItem(FavoriteItemData userFavoritesListItemData)
        {
            if (!ModelState.IsValid)
            {
                var failedResponse = new RequestFailedResponse
                {
                    Success = false,
                    Message = "Invalid ModelState!",
                    StatusCode = HttpStatusCode.BadRequest
                };

                return BadRequest(failedResponse);
            }

            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                var failedResponse = new RequestFailedResponse
                {
                    Success = false,
                    Message = "You must be logged in to add a favorite item.",
                    StatusCode = HttpStatusCode.Unauthorized
                };

                return Unauthorized(failedResponse);
            }


            var favoritesList = await _context.UserFavoritesLists
                .Include(flist=> flist.FavoritedItems)
                .FirstOrDefaultAsync(flist => flist.OwnerId == currentUser.Id);

            if (favoritesList.FavoritedItems == null)
                favoritesList.FavoritedItems = new List<UserFavoritesListItem>();

            if (favoritesList.FavoritedItems.Count >= 10)
            {
                var failedResponse = new RequestFailedResponse
                {
                    Success = false,
                    Message = "As a basic user, you can only have up to 10 favorite servers on your favorites list.",
                    StatusCode = HttpStatusCode.BadRequest
                };

                return BadRequest(failedResponse);
            }

            var favoritesItemToAdd = _mapper.Map<UserFavoritesListItem>(userFavoritesListItemData);
            
            // I realize I am mapping the values over, but I want to be sure the mappings missed anything or left anything null/empty.
            favoritesItemToAdd.OwnerId = currentUser.Id;
            favoritesItemToAdd.ServerName = userFavoritesListItemData.ServerName;
            favoritesItemToAdd.ServerAddress = userFavoritesListItemData.ServerAddress;
            favoritesItemToAdd.ServerPort = userFavoritesListItemData.ServerPort;
            favoritesItemToAdd.ServerEra = userFavoritesListItemData.ServerEra;
            favoritesItemToAdd.PvPEnabled = userFavoritesListItemData.PvPEnabled;
            favoritesItemToAdd.ServerWebsite = userFavoritesListItemData.ServerWebsite;
            favoritesItemToAdd.ServerBanner = userFavoritesListItemData.ServerBanner;
            favoritesItemToAdd.ServerId = userFavoritesListItemData.ServerId;

            favoritesList.FavoritedItems.Add(favoritesItemToAdd);

            _context.Entry(favoritesList).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var failedResponse = new RequestFailedResponse
                {
                    Success = false,
                    Message = $"An error occurred while adding the favorite item: {ex.Message}",
                    StatusCode = HttpStatusCode.BadRequest
                };

                return BadRequest(failedResponse);
            }

            var createdFavoritesListItem = await _context.UserFavoritesListItems
                .FirstOrDefaultAsync(f => f.Id == favoritesItemToAdd.Id);

            var createdFavoritesListItemData = _mapper.Map<FavoriteItemData>(createdFavoritesListItem);

            return CreatedAtAction(nameof(GetUserFavoritesListItemById), new { id = createdFavoritesListItem.Id }, createdFavoritesListItemData);
        }

        [Authorize] // IResponse
        [HttpDelete("list/item/delete/{itemId}")]
        public async Task<ActionResult<IResponse>> DeleteFavoritesItem(int itemId)
        {
            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                var failedResponse = new RequestFailedResponse
                {
                    Success = false,
                    Message = "You must be logged in to do this.",
                    StatusCode = HttpStatusCode.Unauthorized
                };

                return Unauthorized(failedResponse);
            }

            var userFavoritesItem = await _context.UserFavoritesListItems
                .FirstOrDefaultAsync(fli => fli.Id == itemId);

            if( userFavoritesItem == null )
            {
                var failedNotFound = new RequestFailedResponse
                {
                    Success = false,
                    Message = $"Favorites item {itemId} not found.",
                    StatusCode = HttpStatusCode.NotFound
                };

                return NotFound(failedNotFound);
            }

            if (userFavoritesItem.OwnerId != currentUser.Id)
            {
                if (!Permissions.HasElevatedRole(currentUser.Role))
                {
                    var failedResponse = new RequestFailedResponse
                    {
                        Success = false,
                        Message = "You can only edit your own favorites list.",
                        StatusCode = HttpStatusCode.Unauthorized
                    };

                    return Unauthorized(failedResponse);
                }
            }

            _context.UserFavoritesListItems.Remove(userFavoritesItem);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var failedResponse = new RequestFailedResponse
                {
                    Success = false,
                    Message = $"Something happened that no one was prepared for: {ex.Message}",
                    StatusCode = HttpStatusCode.BadRequest
                };

                return BadRequest(failedResponse);
            }

            var successResponse = new BasicSuccessResponse
            {
                Success = true,
                Message = $"Favorites item {itemId} deleted.",
                StatusCode = HttpStatusCode.NoContent
            };

            return Ok(successResponse);
        }

        #endregion

        #region Privileged Endpoints

        [Authorize(Policy = "Privileged")] // IResponse
        [HttpGet("list/view/{id}")]
        public async Task<ActionResult<IResponse>> GetUserFavoritesList(int id)
        {
            var userFavoritesList = await _context.UserFavoritesLists
                .Include(flist => flist.FavoritedItems)
                .FirstOrDefaultAsync(flist => flist.Id == id);

            if (userFavoritesList == null)
            {
                var failedResponse = new RequestFailedResponse
                {
                    Success = false,
                    Message = "Favorites List NOT FOUND.",
                    StatusCode = HttpStatusCode.NotFound
                };

                return NotFound( failedResponse );
            }

            var userFavoritesResponse = new FavoritesDataReponse
            {
                Success = true,
                Message = "Favorites list found.",
                StatusCode = HttpStatusCode.OK,
                Entity = _mapper.Map<FavoritesData>(userFavoritesList)

            };

            return Ok(userFavoritesResponse);
        }

        #endregion
    }
}