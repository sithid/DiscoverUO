using AutoMapper;
using DiscoverUO.Api.Models;
using DiscoverUO.Lib.Shared;
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

        [Authorize]
        [HttpGet("list/view")]
        public async Task<ActionResult<FavoritesData>> GetUserFavoritesLists()
        {
            var currentUser = await Permissions.GetCurrentUser(this.User, _context);
            var currentUserFavorites = await _context.UserFavoritesLists
                .Include(flist => flist.FavoritedItems)
                .FirstOrDefaultAsync(flist => flist.OwnerId == currentUser.Id);

            var favoritesListDto = _mapper.Map<FavoritesData>(currentUser.Favorites);

            return Ok(favoritesListDto);
        }

        [Authorize]
        [HttpGet("list/item/view/{id}")]
        public async Task<ActionResult<FavoriteItemData>> GetUserFavoritesListItem(int id)
        {
            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                return Unauthorized($"You must be logged in to do this.");
            }

            var favoritesListItem = await _context.UserFavoritesListItems
                .FirstOrDefaultAsync(f => f.Id == id);

            if (favoritesListItem == null)
            {
                return NotFound("The favorites list item was not found.");
            }

            if (currentUser.Id != favoritesListItem.OwnerId)
            {
                if (!Permissions.HasElevatedRole(currentUser.Role))
                {
                    return Unauthorized("You do not have permission to edit that user.");
                }
            }

            var favoritesListItemDto = _mapper.Map<FavoriteItemData>(favoritesListItem);

            return Ok(favoritesListItemDto);
        }

        [Authorize]
        [HttpPut("list/item/update/{id}")]
        public async Task<ActionResult<FavoriteItemData>> UpdateFavoritesListItem(int id, FavoriteItemData userFavoritesListItemDto)
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
                .FirstOrDefaultAsync(fListItem => fListItem.Id == id);

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

            userFavoritesListItem.ServerName = userFavoritesListItemDto.ServerName;
            userFavoritesListItem.ServerAddress = userFavoritesListItemDto.ServerAddress;
            userFavoritesListItem.ServerPort = userFavoritesListItemDto.ServerPort;
            userFavoritesListItem.ServerEra = userFavoritesListItemDto.ServerEra;
            userFavoritesListItem.PvPEnabled = userFavoritesListItemDto.PvPEnabled;
            userFavoritesListItem.ServerWebsite = userFavoritesListItemDto.ServerWebsite;
            userFavoritesListItem.ServerBanner = userFavoritesListItemDto.ServerBanner;

            _context.Entry(userFavoritesListItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return BadRequest($"Something happened that no one was prepared for. {ex}");
            }

            var updatedUserFavoritesListItem = await _context.UserFavoritesListItems
                .FirstOrDefaultAsync(fListItem => fListItem.Id == id);

            userFavoritesListItemDto = _mapper.Map<FavoriteItemData>(updatedUserFavoritesListItem);

            return Ok(userFavoritesListItemDto);
        }

        [Authorize]
        [HttpPost("list/item/add")]
        public async Task<ActionResult<IResponse>> AddFavoritesItem(FavoriteItemData userFavoritesListItemDto)
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

            var favoritesItemToAdd = _mapper.Map<UserFavoritesListItem>(userFavoritesListItemDto);

            _context.UserFavoritesListItems.Add(favoritesItemToAdd);

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
                    StatusCode = HttpStatusCode.Unauthorized
                };

                return BadRequest(failedResponse);
            }

            var createdFavoritesListItem = await _context.UserFavoritesListItems
                .FirstOrDefaultAsync(f => f.Id == favoritesItemToAdd.Id);

            var createdFavoritesListItemDto = _mapper.Map<FavoriteItemData>(createdFavoritesListItem);

            var itemCreated = new FavoriteItemDataReponse
            {
                Success = true,
                Message = "Favorites item created successfully.",
                StatusCode = HttpStatusCode.OK,
                Entity = createdFavoritesListItemDto
            };

            return Ok(itemCreated);
        }

        [Authorize]
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
                .Include(fli => fli.Id == itemId)
                .FirstOrDefaultAsync(fli => fli.Id == itemId);

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
                    StatusCode = HttpStatusCode.Unauthorized
                };

                return BadRequest(failedResponse);
            }

            var successResponse = new BasicSuccessResponse
            {
                Success = true,
                Message = "Favorites List Item deleted.",
                StatusCode = HttpStatusCode.NoContent
            };

            return Ok(successResponse);
        }

        #endregion

        #region Privileged Endpoints

        [Authorize(Policy = "Privileged")]
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

            var userFavoritesResponse = new FavoritesDataListReponse
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