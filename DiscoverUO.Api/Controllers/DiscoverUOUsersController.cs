using Microsoft.AspNetCore.Mvc;
using DiscoverUO.Api.Models;
using DiscoverUO.Api.Data.Repositories.Contracts;

namespace DiscoverUO.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscoverUOUsersController : ControllerBase
    {
        private readonly IUserDataRepository _dataRepository;

        public DiscoverUOUsersController(IUserDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }

        [HttpGet("GetUsers")]
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            try
            {
                var userList = await _dataRepository.GetUsers();

                if (userList == null || userList.Count == 0)
                    return NotFound("No users found.");
                else
                    return userList;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetUser/{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            try
            {
                var user = await _dataRepository.GetUser(id); ;

                if (user == null)
                    return NotFound($"The user you are looking for, with Id = {id}, was not found.");
                else
                    return user;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("PutUser/{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            try
            {
                if (id != user.Id)
                    return BadRequest($"User Id = {id} does not match Id = {user.Id} of the user you are updating. Id's much match.");

                if (!await _dataRepository.UserExists(id))
                    return NotFound("That user doesn't exist.");

                var success = await _dataRepository.PutUser(user);

                if (!success)
                    return BadRequest();

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("PostUser")]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            try
            {
                var success = await _dataRepository.PostUser(user);

                if (success)
                    return CreatedAtAction("GetUser", new { id = user.Id }, user);
                else
                    return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!await _dataRepository.UserExists(id))
                return NotFound();

            bool success = await _dataRepository.DeleteUser(id);

            if (!success)
                return BadRequest();

            return NoContent();
        }
    }
}
