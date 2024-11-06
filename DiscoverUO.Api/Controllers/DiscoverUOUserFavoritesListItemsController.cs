using Microsoft.AspNetCore.Mvc;
using DiscoverUO.Api.Models;
using DiscoverUO.Api.Data.Repositories.Contracts;

namespace DiscoverUO.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscoverUOUserFavoritesListItemsController : ControllerBase
    {
        private readonly IUserFavoritesListItemDataRepository _dataRepository;

        public DiscoverUOUserFavoritesListItemsController(IUserFavoritesListItemDataRepository userFavoritesListItemDataRepository)
        {
            _dataRepository = userFavoritesListItemDataRepository;
        }

        [HttpGet("GetUserFavoritesListItems")]
        public async Task<ActionResult<List<UserFavoritesListItem>>> GetUserFavoritesListItems()
        {
            try
            {
                var userFavoritesListItems = await _dataRepository.GetUserFavoritesListItems();

                if (userFavoritesListItems == null || userFavoritesListItems.Count == 0)
                    return NotFound("No favorites list items found.");
                else
                    return userFavoritesListItems;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetUserFavoritesListItem/{id}")]
        public async Task<ActionResult<UserFavoritesListItem>> GetUserFavoritesListItem(int id)
        {
            try
            {
                var userFavoritesList = await _dataRepository.GetUserFavoritesListItem(id); ;

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

        [HttpPut("PutUserFavoritesListItem/{id}")]
        public async Task<IActionResult> PutUserFavoritesListItem(int id, UserFavoritesListItem userFavoritesListItem)
        {
            try
            {
                if (id != userFavoritesListItem.Id)
                    return BadRequest($"User Id = {id} does not match Id = {userFavoritesListItem.Id} of the user you are updating. Id's much match.");

                if (!await _dataRepository.UserFavoritesListItemExists(id))
                    return NotFound("That user doesn't exist.");

                var success = await _dataRepository.PutUserFavoritesListItem(userFavoritesListItem);

                if (!success)
                    return BadRequest();

                return Ok(userFavoritesListItem);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("PostUserFavoritesListItem")]
        public async Task<ActionResult<UserFavoritesListItem>> PostUserFavoritesListItem(UserFavoritesListItem userFavoritesListItem)
        {
            try
            {
                var success = await _dataRepository.PostUserFavoritesListItem(userFavoritesListItem);

                if (success)
                    return CreatedAtAction("GetUserFavoritesListItem", new { id = userFavoritesListItem.Id }, userFavoritesListItem);
                else
                    return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpDelete("DeleteUserFavoritesListItem/{id}")]
        public async Task<IActionResult> DeleteUserFavoritesListItem(int id)
        {
            if (!await _dataRepository.UserFavoritesListItemExists(id))
                return NotFound();

            bool success = await _dataRepository.DeleteUserFavoritesListItem(id);

            if (!success)
                return BadRequest();

            return NoContent();
        }
    }
}
