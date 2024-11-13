using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiscoverUO.Api.Models;
using DiscoverUO.Lib.DTOs.Favorites;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using DiscoverUO.Lib.DTOs.Servers;
using DiscoverUO.Lib.DTOs.Users;
using System.Security.Claims;

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
        public async Task<ActionResult<UserFavoritesListDto>> GetUserFavoritesLists()
        {
            var currentUser = await Permissions.GetCurrentUser( this.User, _context );
            var currentUserFavorites = await _context.UserFavoritesLists
                .Include(flist => flist.FavoritedItems)
                .FirstOrDefaultAsync(flist => flist.OwnerId == currentUser.Id);

            var favoritesListDto = _mapper.Map<UserFavoritesListDto>(currentUser.Favorites);

            return Ok(favoritesListDto);
        }

        [Authorize]
        [HttpGet("list/item/view/{id}")]
        public async Task<ActionResult<UserFavoritesListItemDto>> GetUserFavoritesListItem(int id)
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

            var favoritesListItemDto = _mapper.Map<UserFavoritesListItemDto>(favoritesListItem);

            return Ok(favoritesListItemDto);
        }

        [Authorize]
        [HttpPut("list/item/update/{id}")]
        public async Task<ActionResult<UserFavoritesListItemDto>> UpdateFavoritesListItem(int id, UserFavoritesListItemDto userFavoritesListItemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                return Unauthorized($"You must be logged in to do this.");
            }

            var userFavoritesListItem = await _context.UserFavoritesListItems
                .FirstOrDefaultAsync(fListItem => fListItem.Id == id);

            if ( userFavoritesListItem == null )
            {
                return NotFound("The favorites list item was not found.");
            }

            if (userFavoritesListItem.OwnerId != currentUser.Id)
            {
                if (!Permissions.HasElevatedRole(currentUser.Role))
                {
                    return Unauthorized("You can only edit your own favorites list.");
                }
            }

            userFavoritesListItem.ServerName = userFavoritesListItem.ServerName;
            userFavoritesListItem.ServerAddress = userFavoritesListItemDto.ServerAddress;
            userFavoritesListItem.ServerPort = userFavoritesListItemDto.ServerPort;
            userFavoritesListItem.ServerEra = userFavoritesListItemDto.ServerEra;
            userFavoritesListItem.PvPEnabled = userFavoritesListItemDto.PvPEnabled;

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

            userFavoritesListItemDto = _mapper.Map<UserFavoritesListItemDto>(updatedUserFavoritesListItem);

            return Ok(userFavoritesListItemDto);
        }
        
        [Authorize]
        [HttpPost("list/item/add")]
        public async Task<ActionResult<UserFavoritesListItemDto>> AddFavoritesItem(UserFavoritesListItemDto userFavoritesListItemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                return Unauthorized($"You must be logged in to add a favorite item.");
            }

            var favoritesItemToAdd = _mapper.Map<UserFavoritesListItem>(userFavoritesListItemDto);

            _context.UserFavoritesListItems.Add(favoritesItemToAdd);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"An error occurred while adding the favorite item: {ex.Message}");
            }

            var createdFavoritesListItem = await _context.UserFavoritesListItems
                .FirstOrDefaultAsync(f => f.Id == favoritesItemToAdd.Id);

            var createdFavoritesListItemDto = _mapper.Map<UserFavoritesListItemDto>(createdFavoritesListItem);

            return CreatedAtRoute( "GetUserFavoritesListItem", new { id = createdFavoritesListItem.Id }, createdFavoritesListItemDto);
        }

        [Authorize]
        [HttpDelete("list/item/delete/{itemId}")]
        public async Task<ActionResult<UserFavoritesListItemDto>> DeleteFavoritesItem(int itemId )
        {
            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                return Unauthorized($"You must be logged in to do this.");
            }

            var userFavoritesItem = await _context.UserFavoritesListItems
                .Include(fli => fli.Id == itemId)
                .FirstOrDefaultAsync(fli => fli.Id == itemId);

            if (userFavoritesItem.OwnerId != currentUser.Id)
            {
                if (!Permissions.HasElevatedRole(currentUser.Role))
                {
                    return Unauthorized("You can only edit your own favorites list.");
                }
            }

            _context.UserFavoritesListItems.Remove(userFavoritesItem);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return BadRequest($"Something happened that no one was prepared for: {ex}");
            }

            return NoContent();
        }

        #endregion

        #region Privileged Endpoints

        [Authorize(Policy = "Privileged")]
        [HttpGet("list/view/{id}")]
        public async Task<ActionResult<UserFavoritesListDto>> GetUserFavoritesList(int id)
        {
            var userFavoritesList = await _context.UserFavoritesLists
                .Include( flist => flist.FavoritedItems )
                .FirstOrDefaultAsync(flist => flist.Id == id);

            if (userFavoritesList == null)
            {
                return NotFound();
            }

            var userFavoritesListDto = _mapper.Map<Lib.DTOs.Favorites.UserFavoritesListDto>(userFavoritesList);

            return Ok(userFavoritesListDto);
        }

        #endregion
    }
}