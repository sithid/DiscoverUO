using AutoMapper;
using DiscoverUO.Api.Models;
using DiscoverUO.Lib.DTOs.Servers;
using DiscoverUO.Lib.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DiscoverUO.Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class DiscoverUOUsersController : ControllerBase
    {
        #region Private Fields

        private readonly DiscoverUODatabaseContext _context;
        private readonly IMapper _mapper;
        private readonly string secreteKey;

        #endregion

        public DiscoverUOUsersController(DiscoverUODatabaseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;

            secreteKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

            if (string.IsNullOrEmpty(secreteKey)) {
                secreteKey = "temp_secret_key_for_initial_setup";
            }
        }

        #region AllowAnonymous Endpoints

        [AllowAnonymous]
        [HttpPost("Authenticate")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.UserName == loginDto.UserName);

            if (user == null)
            {
                return Unauthorized("Invalid username or password.");

            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid username or password.");
            }

            var token = GenerateToken(user);

            return Ok(new { Token = token });
        }

        [AllowAnonymous]
        [HttpPost("CreateUser")]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createdUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _mapper.Map<User>(createdUserDto);

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(createdUserDto.Password);

            var userProfile = new UserProfile { OwnerId = user.Id, UserDisplayName = user.UserName };
            user.Profile = userProfile;

            var favoritesList = new UserFavoritesList { OwnerId = user.Id };
            user.Favorites = favoritesList;

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            var createdUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            var userDto = _mapper.Map<User>(createdUser);

            return CreatedAtAction("GetUserById", new { id = createdUser.Id }, userDto);

        }

        #endregion

        #region BasicUser Endpoints

        [Authorize]
        [HttpGet("ById/{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            var user = await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.Favorites)
                .FirstOrDefaultAsync(u => u.Id == id);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userProfile = new UserProfile { OwnerId = user.Id, UserDisplayName = user.UserName };
            _context.UserProfiles.Add(userProfile);

            var favoritesList = new UserFavoritesList { OwnerId = user.Id };
            _context.UserFavoritesLists.Add(favoritesList);

            if (user == null)
            {
                return NotFound();
            }

            var userDto = _mapper.Map<UserDto>(user);

            return Ok(userDto);
        }

        [Authorize]
        [HttpGet("ByName/{userName}")]
        public async Task<ActionResult<UserDto>> GetUserByName(string userName)
        {
            var user = await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.Favorites)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null)
            {
                return NotFound();
            }

            var userDto = _mapper.Map<UserDto>(user);

            return Ok(userDto);
        }

        [Authorize]
        [HttpPut("UpdateUser/{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateUserDto)
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

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            if (currentUser.Id != user.Id)
            {
                if (!UserUtilities.HasElevatedRole(currentUser.Role) && !UserUtilities.HasHigherPermission(currentUser.Role, user.Role))
                {
                    return Unauthorized("You do not have permission to edit that user.");
                }
            }

            user.UserName = updateUserDto.UserName;
            user.Email = updateUserDto.Email;

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return BadRequest($"Something happened that noone was prepared for.\n\r{ex}");
            }

            var updatedUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            var updatedUserDto = _mapper.Map<UserDto>(updatedUser);

            return Ok(updatedUserDto);
        }

        [Authorize]
        [HttpPut("UpdatePassword/{id}")]
        public async Task<IActionResult> UpdatePassword(int id, UpdateUserPasswordDto updatePasswordDto)
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

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            if (currentUser.Id != user.Id)
            {
                if (!UserUtilities.HasElevatedRole(currentUser.Role) && !UserUtilities.HasHigherPermission(currentUser.Role, user.Role))
                {
                    return Unauthorized("You do not have permission to edit that user.");
                }
            }

            if (!BCrypt.Net.BCrypt.Verify(updatePasswordDto.CurrentPassword, user.PasswordHash))
            {
                return Unauthorized("Invalid password.");
            }


            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updatePasswordDto.NewPassword);

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return BadRequest($"Something happened that noone was prepared for.\n\r{ex}");
            }

            var updatedUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            var updatedUserDto = _mapper.Map<UserDto>(updatedUser);

            return Ok(updatedUserDto);
        }

        #endregion

        #region Privileged Endpoints

        [Authorize(Policy = "Privileged")]
        [HttpPut("admin/ChangeRemainingVotes/{id}")]
        public async Task<IActionResult> ChangeRemainingVotes(int id, int votesRemaining)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            user.DailyVotesRemaining = votesRemaining;

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {

                return BadRequest($"Something happened that noone was prepared for.\n\r{ex}");

            }

            var updatedUser = await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.Favorites)
                .FirstOrDefaultAsync(u => u.Id == id);

            var updatedUserDto = _mapper.Map<UserDto>(updatedUser);

            return Ok(updatedUserDto);
        }

        [Authorize(Policy = "Privileged")]
        [HttpDelete("admin/DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var currentUser = await GetCurrentUser();

            if (currentUser == null)
            {
                return Unauthorized($"You must be logged in to do this.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            if (currentUser.Id != user.Id)
            {
                if (!UserUtilities.HasElevatedRole(currentUser.Role))
                {
                    return Unauthorized("You do not have permission to do that.");
                }
            }

            if (!UserUtilities.HasHigherPermission(currentUser.Role, user.Role))
            {
                return Unauthorized("You do not have permission to do that.");
            }

            var newServerOwner = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "Admin");

            if (newServerOwner == null)
            {
                newServerOwner = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "Owner");

                if (newServerOwner == null)
                {
                    return StatusCode(500, "No suitable server owner found.");
                }
            }

            await _context.Servers
                .Where(s => s.OwnerId == id)
                .ExecuteUpdateAsync(s => s.SetProperty(server => server.OwnerId, newServerOwner.Id));

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        #endregion

        #region Admin Endpoints

        [Authorize(Policy = "OwnerOrAdmin")]
        [HttpGet("All")]
        public async Task<ActionResult<List<UserDto>>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.Favorites)
                .ToListAsync();

            var userDtos = _mapper.Map<List<UserDto>>(users);

            return Ok(userDtos);
        }

        [Authorize(Policy = "OwnerOrAdmin")]
        [HttpPost("admin/CreateUserWithRole")]
        public async Task<ActionResult<UserDto>> CreateUserWithRole(CreateUserWithRoleDto createdUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _mapper.Map<User>(createdUserDto);

            var userProfile = new UserProfile { OwnerId = user.Id, UserDisplayName = user.UserName };
            user.Profile = userProfile;

            var favoritesList = new UserFavoritesList { OwnerId = user.Id };
            user.Favorites = favoritesList;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var createdUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            var userDto = _mapper.Map<UserDto>(createdUser);

            return CreatedAtAction("GetUserById", new { id = createdUser.Id }, userDto);

        }

        [Authorize(Policy = "OwnerOrAdmin")]
        [HttpPut("admin/UpdateUserRole/{id}")]
        public async Task<IActionResult> UpdateUserRole(int id, string role)
        {
            var currentUser = await GetCurrentUser();

            if (currentUser == null)
            {
                return Unauthorized($"You must be logged in to do this.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            if (currentUser.Id != user.Id)
            {
                if (!UserUtilities.HasElevatedRole(currentUser.Role))
                {
                    return Unauthorized("You do not have permission to do that.");
                }
            }

            if( !UserUtilities.HasHigherPermission( currentUser.Role, role ))
            {
                return Unauthorized("You do not have permission to do that.");
            }

            user.Role = role;

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the server owner: {ex.Message}");
            }

            var updatedUser = await _context.Users
                 .FirstOrDefaultAsync(u => u.Id == id);

            var updatedUserDto = _mapper.Map<UserDto>(updatedUser);

            return Ok(updatedUserDto);
        }
        #endregion

        #region Endpoint Utilities

        private string GenerateToken(User requester)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secreteKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, requester.Id.ToString()),
                new Claim(ClaimTypes.Role, requester.Role),
                new Claim(ClaimTypes.Name, requester.UserName)

            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)

            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task<User> GetCurrentUser()
        {
            var userId = await UserUtilities.GetCurrentUserId(this.User);

            var currentUser = await _context.Users
                .Include(u => u.Favorites)
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(user => user.Id == userId);

            return currentUser;
        }

        #endregion
    }
}
