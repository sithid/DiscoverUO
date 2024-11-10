using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiscoverUO.Api.Models;
using DiscoverUO.Lib.DTOs.Favorites;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using DiscoverUO.Lib.DTOs.Servers;

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

        public DiscoverUOUserFavoritesListsController( DiscoverUODatabaseContext context, IMapper mapper )
        {
            _context = context;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet("view")]
        public async Task<ActionResult<IEnumerable<UserFavoritesListDto>>> GetUserFavoritesLists()
        {
            var currentUser = await GetCurrentUser();

            if (currentUser.Favorites == null )
            {
                return NotFound();
            }

            var favoritesListDto = _mapper.Map<UserFavoritesListDto>(currentUser.Favorites);

            return Ok(favoritesListDto);
        }
        
        [Authorize(Policy = "Privileged")]
        [HttpGet("view/{id}")]
        public async Task<ActionResult<UserFavoritesListDto>> GetUserFavoritesList(int id)
        {
            var userFavoritesList = await _context.UserFavoritesLists
                .FirstOrDefaultAsync(flist => flist.Id == id);

            if (userFavoritesList == null)
            {
                return NotFound();
            }

            var userFavoritesListDto = _mapper.Map<Lib.DTOs.Favorites.UserFavoritesListDto>(userFavoritesList);

            return Ok(userFavoritesListDto);
        }

        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<ActionResult<UserFavoritesListDto>> UpdateFavoritesList(int id, UserFavoritesListDto userFavoritesListDto)
        {
            var userFavoritesList = await _context.UserFavoritesLists
                .FirstOrDefaultAsync(flist => flist.Id == id);

            userFavoritesList = _mapper.Map<UserFavoritesList>(userFavoritesListDto);

            _context.Entry(userFavoritesList).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return BadRequest($"Something happened that no one was prepared for.");
            }

            var updatedUserFavoritesList = await _context.UserFavoritesLists
                .FirstOrDefaultAsync(flist => flist.Id == id);
                
            userFavoritesListDto = _mapper.Map<UserFavoritesListDto>(updatedUserFavoritesList);

            return Ok(userFavoritesListDto);
        }

        #region Endpoint Utilities

        private async Task<User> GetCurrentUser()
        {
            var userId = await Permissions.GetCurrentUserId(this.User);

            var currentUser = await _context.Users
                .Include(u => u.Favorites)
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(user => user.Id == userId);

            return currentUser;
        }

        #endregion
    }
}