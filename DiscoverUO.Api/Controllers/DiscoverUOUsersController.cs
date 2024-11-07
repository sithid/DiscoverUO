using Microsoft.AspNetCore.Mvc;
using DiscoverUO.Api.Models;
using DiscoverUO.Lib.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DiscoverUO.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscoverUOUsersController : ControllerBase
    {
        private readonly DiscoverUODatabaseContext _context;

        public DiscoverUOUsersController(DiscoverUODatabaseContext context)
        {
            _context = context;
        }

        [HttpPost("CreateUser")]
        public async Task<ActionResult<UserDto>> CreateUser(UserDto userDto)
        {
            var newUser = new User
            {
                UserName = userDto.UserName,
                
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            var createdUser = await _context.Users
                .Include(user => user.ServersAdded)
                .Include(user => user.Profile)
                .Include(user => user.Favorites)
                .FirstOrDefaultAsync(user => user.Id == newUser.Id);

            var createdUserDto = new UserDto
            {
                Id = createdUser.Id,
                UserName = createdUser.UserName,
                ServersAddedIds = createdUser.ServersAdded?.Select(s => s.Id).ToList(),
                ProfileId = createdUser.Profile?.Id,
                FavoritesId = createdUser.Favorites?.Id
            };

            return CreatedAtAction("GetUser", new { id = createdUserDto.Id }, createdUserDto);
        }

        [HttpGet("GetUser/{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.ServersAdded)
                .Include(u => u.Profile)
                .Include(u => u.Favorites)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                DailyVotesRemaining = user.DailyVotesRemaining,
                ServersAddedIds = user.ServersAdded.Select(s => s.Id).ToList(),
                ProfileId = user.Profile?.Id,
                FavoritesId = user.Favorites?.Id
            };

            return Ok(userDto);
        }
    }
}
