using Microsoft.AspNetCore.Mvc;
using DiscoverUO.Api.Models;
using DiscoverUO.Api.Data.Repositories.Contracts;

namespace DiscoverUO.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscoverUOUserFavoritesListsController : ControllerBase
    {
        private readonly IUserFavoritesListDataRepository _dataRepository;

        public DiscoverUOUserFavoritesListsController(IUserFavoritesListDataRepository userFavoritesListDataRepository)
        {
            _dataRepository = userFavoritesListDataRepository;
        }

        [HttpGet("GetUserFavoritesLists")]
        public async Task<ActionResult<List<UserFavoritesList>>> GetUserFavoritesLists()
        {
            try
            {
                var userFavoritesLists = await _dataRepository.GetUserFavoritesLists();

                if (userFavoritesLists == null || userFavoritesLists.Count == 0)
                    return NotFound("No favorites lists found.");
                else
                    return userFavoritesLists;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetUserFavoritesList/{id}")]
        public async Task<ActionResult<UserFavoritesList>> GetUserFavoritesList(int id)
        {
            try
            {
                var userFavoritesList = await _dataRepository.GetUserFavoritesList(id); ;

                if (userFavoritesList == null)
                    return NotFound($"The user favorites list you are looking for, with Id = {id}, was not found.");
                else
                    return userFavoritesList;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("PutUserFavoritesList/{id}")]
        public async Task<IActionResult> PutUserFavoritesList(int id, UserFavoritesList userFavoritesList)
        {
            try
            {
                if (id != userFavoritesList.Id)
                    return BadRequest($"User Id = {id} does not match Id = {userFavoritesList.Id} of the user you are updating. Id's much match.");

                if (!await _dataRepository.UserFavoritesListExists(id))
                    return NotFound("That user doesn't exist.");

                var success = await _dataRepository.PutUserFavoritesList(userFavoritesList);

                if (!success)
                    return BadRequest();

                return Ok(userFavoritesList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("PostUserFavoritesList")]
        public async Task<ActionResult<UserProfile>> PostUserFavoritesList(UserFavoritesList userFavoritesList)
        {
            try
            {
                var success = await _dataRepository.PostUserFavoritesList(userFavoritesList);

                if (success)
                    return CreatedAtAction("GetUserFavoritesList", new { id = userFavoritesList.Id }, userFavoritesList);
                else
                    return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpDelete("DeleteUserFavoritesList/{id}")]
        public async Task<IActionResult> DeleteUserFavoritesList(int id)
        {
            if (!await _dataRepository.UserFavoritesListExists(id))
                return NotFound();

            bool success = await _dataRepository.DeleteUserFavoritesList(id);

            if (!success)
                return BadRequest();

            return NoContent();
        }
    }
}
