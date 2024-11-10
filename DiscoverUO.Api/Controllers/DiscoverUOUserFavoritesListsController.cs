using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiscoverUO.Api.Models;
using DiscoverUO.Lib.DTOs.Favorites;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using DiscoverUO.Lib.DTOs.Servers;
using DiscoverUO.Lib.DTOs.Users;

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
            var currentUser = await GetCurrentUser();
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
            var currentUser = await GetCurrentUser();

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

            var currentUser = GetCurrentUser();

            var userFavoritesList = await _context.UserFavoritesLists
                .Include(flist => flist.FavoritedItems)
                .FirstOrDefaultAsync(flist => flist.OwnerId == currentUser.Id);

            if (userFavoritesList == null)
            {
                return BadRequest();
            }

            var userFavoritesListItem  = userFavoritesList.FavoritedItems.FirstOrDefault( flItem => flItem.Id == id);

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
                return BadRequest($"Something happened that no one was prepared for.");
            }

            var updatedUserFavoritesListItem = await _context.UserFavoritesListItems
                .FirstOrDefaultAsync(flist => flist.Id == id);

            userFavoritesListItemDto = _mapper.Map<UserFavoritesListItemDto>(updatedUserFavoritesListItem);

            return Ok(userFavoritesListItemDto);
        }

        [Authorize]
        [HttpPost("list/item/add")]
        public async Task<IActionResult> AddFavoriteListItem(UserFavoritesListItemDto favoritesListItemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = await GetCurrentUser();

            if (currentUser == null)
            {
                return Unauthorized($"You must be logged in to do this.");
            }

            if (currentUser.Favorites == null || currentUser.Favorites.FavoritedItems == null )
                Console.WriteLine("Favorites List is Null or Favorites List Items is Null!");

            var favoritesListItem = new UserFavoritesListItem { OwnerId = currentUser.Id, FavoritesListId = currentUser.Favorites.Id };

            favoritesListItem.ServerName = favoritesListItemDto.ServerName;
            favoritesListItem.ServerAddress = favoritesListItemDto.ServerAddress;
            favoritesListItem.ServerPort = favoritesListItemDto.ServerPort;
            favoritesListItem.ServerEra = favoritesListItemDto.ServerEra;
            favoritesListItem.PvPEnabled = favoritesListItemDto.PvPEnabled;

            currentUser.Favorites.FavoritedItems.Add(favoritesListItem);

            _context.Entry(currentUser.Favorites).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return BadRequest($"Something happened that no one was prepared for.");
            }

            var createdFavoritesListItem = await _context.UserFavoritesListItems
                .FirstOrDefaultAsync(uflist => uflist.OwnerId == currentUser.Id);

            var createdFavoritesItemDto = _mapper.Map<UserFavoritesListItemDto>(createdFavoritesListItem);

            return CreatedAtAction("GetUserFavoritesListItem", new { id = createdFavoritesListItem.Id }, createdFavoritesItemDto);
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

        #region Endpoint Utilities

        private async Task<User> GetCurrentUser()
        {
            var userId = await Permissions.GetCurrentUserId(this.User);

            var currentUser = await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.Favorites)
                .ThenInclude(favs => favs.FavoritedItems )
                .FirstOrDefaultAsync(user => user.Id == userId);

            return currentUser;
        }
        #endregion
    }
}