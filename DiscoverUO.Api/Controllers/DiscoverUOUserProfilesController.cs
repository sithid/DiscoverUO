using Microsoft.AspNetCore.Mvc;
using DiscoverUO.Api.Models;
using DiscoverUO.Api.Data.Repositories.Contracts;

namespace DiscoverUO.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscoverUOUserProfilesController : ControllerBase
    {
        private readonly IUserProfileDataRepository _dataRepository;

        public DiscoverUOUserProfilesController(IUserProfileDataRepository userProfileDataRepository)
        {
            _dataRepository = userProfileDataRepository;
        }

        [HttpGet("GetUserProfiles")]
        public async Task<ActionResult<List<UserProfile>>> GetUserProfiles()
        {
            try
            {
                var userProfileList = await _dataRepository.GetUserProfiles();

                if (userProfileList == null || userProfileList.Count == 0)
                    return NotFound("No users profiles found.");
                else
                    return userProfileList;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetUserProfile/{id}")]
        public async Task<ActionResult<UserProfile>> GetUserProfile(int id)
        {
            try
            {
                var user = await _dataRepository.GetUserProfile(id); ;

                if (user == null)
                    return NotFound($"The user profile you are looking for, with Id = {id}, was not found.");
                else
                    return user;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("PutUserProfile/{id}")]
        public async Task<IActionResult> PutUserProfile(int id, UserProfile userProfile)
        {
            try
            {
                if (id != userProfile.Id)
                    return BadRequest($"User Id = {id} does not match Id = {userProfile.Id} of the user you are updating. Id's much match.");

                if (!await _dataRepository.UserProfileExists(id))
                    return NotFound("That user doesn't exist.");

                var success = await _dataRepository.PutUserProfile(userProfile);

                if (!success)
                    return BadRequest();

                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("PostUserProfile")]
        public async Task<ActionResult<UserProfile>> PostUserProfile(UserProfile userProfile)
        {
            try
            {
                var success = await _dataRepository.PostUserProfile(userProfile);

                if (success)
                    return CreatedAtAction("GetUserProfile", new { id = userProfile.Id }, userProfile);
                else
                    return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpDelete("DeleteUserProfile/{id}")]
        public async Task<IActionResult> DeleteUserProfile(int id)
        {
            if (!await _dataRepository.UserProfileExists(id))
                return NotFound();

            bool success = await _dataRepository.DeleteUserProfile(id);

            if (!success)
                return BadRequest();

            return NoContent();
        }
    }
}
