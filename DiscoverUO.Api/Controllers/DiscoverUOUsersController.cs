using AutoMapper;
using DiscoverUO.Api.Models;
using DiscoverUO.Lib.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DiscoverUO.Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class DiscoverUOUsersController : ControllerBase
    {
        private readonly DiscoverUODatabaseContext _context;
        private readonly IMapper _mapper;
        private readonly string secreteKey;

        public DiscoverUOUsersController(DiscoverUODatabaseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;

            secreteKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

            if (string.IsNullOrEmpty(secreteKey))
            {
                secreteKey = "temp_secret_key_for_initial_setup";
                Console.WriteLine("JWT secret key not found in environment variables.");
                Console.WriteLine("Using a temporary key.  Fix this immediately.");
            }
        }

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

        [Authorize(Policy = "OwnerOrAdmin")]
        [HttpGet("All")]
        public async Task<ActionResult<List<UserDto>>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.ServersAdded)
                .Include(u => u.Profile)
                .Include(u => u.Favorites)
                .ToListAsync();

            var userDtos = _mapper.Map<List<UserDto>>(users);

            return Ok(userDtos);
        }

        [Authorize]
        [HttpGet("ById/{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
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

            var userDto = _mapper.Map<UserDto>(user);

            return Ok(userDto);
        }

        [Authorize]
        [HttpGet("ByName/{userName}")]
        public async Task<ActionResult<UserDto>> GetUserByName( string userName )
        {
            var user = await _context.Users
                .Include(u => u.ServersAdded)
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

        [AllowAnonymous]
        [HttpPost("CreateUser")]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _mapper.Map<User>(createUserDto);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userProfile = new UserProfile { OwnerId = user.Id, UserDisplayName = user.UserName };
            _context.UserProfiles.Add(userProfile);

            var favoritesList = new UserFavoritesList { OwnerId = user.Id };
            _context.UserFavoritesLists.Add(favoritesList);

            await _context.SaveChangesAsync();

            var createdUser = await _context.Users
                .Include(u => u.ServersAdded)
                .Include(u => u.Profile)
                .Include(u => u.Favorites)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            var userDto = _mapper.Map<User>(createdUser);

            return CreatedAtAction("GetUserById", new { id = createdUser.Id}, userDto);

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

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userProfile = new UserProfile { OwnerId = user.Id, UserDisplayName = user.UserName };
            _context.UserProfiles.Add(userProfile);

            var favoritesList = new UserFavoritesList { OwnerId = user.Id };
            _context.UserFavoritesLists.Add(favoritesList);

            await _context.SaveChangesAsync();

            var createdUser = await _context.Users
                .Include(u => u.ServersAdded)
                .Include(u => u.Profile)
                .Include(u => u.Favorites)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            var userDto = _mapper.Map<UserDto>(createdUser);

            return CreatedAtAction("GetUserById", new { id = createdUser.Id }, userDto);

        }

        [Authorize]
        [HttpPut("UpdateUser/{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserUpdateDto updateUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            user = _mapper.Map<User>(updateUserDto);

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return BadRequest($"Something happened that noone was prepared for.\n\r{ex}");
                }
            }

            var updatedUser = await _context.Users
                .Include(u => u.ServersAdded)
                .Include(u => u.Profile)
                .Include(u => u.Favorites)
                .FirstOrDefaultAsync(u => u.Id == id);

            var updatedUserDto = _mapper.Map<UserDto>(updatedUser);

            return Ok(updatedUserDto);
        }

        [Authorize]
        [HttpPut("UpdatePassword/{id}")]
        public async Task<IActionResult> UpdatePassword(int id, UserUpdatePasswordDto updatePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
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
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return BadRequest($"Something happened that noone was prepared for.\n\r{ex}");
                }
            }

            var updatedUser = await _context.Users
                .Include(u => u.ServersAdded)
                .Include(u => u.Profile)
                .Include(u => u.Favorites)
                .FirstOrDefaultAsync(u => u.Id == id);

            var updatedUserDto = _mapper.Map<UserDto>(updatedUser);

            return Ok(updatedUserDto);
        }

        [Authorize(Policy = "OwnerOrAdmin")]
        [HttpPut("admin/UpdateUserRole/{id}")]
        public async Task<IActionResult> UpdateUserRole(int id, string role)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
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
                .Include(u => u.ServersAdded)
                .Include(u => u.Profile)
                .Include(u => u.Favorites)
                .FirstOrDefaultAsync(u => u.Id == id);

            var updatedUserDto = _mapper.Map<UserDto>(updatedUser);

            return Ok(updatedUserDto);
        }

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
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return BadRequest($"Something happened that noone was prepared for.\n\r{ex}");
                }
            }

            var updatedUser = await _context.Users
                .Include(u => u.ServersAdded)
                .Include(u => u.Profile)
                .Include(u => u.Favorites)
                .FirstOrDefaultAsync(u => u.Id == id);

            var updatedUserDto = _mapper.Map<UserDto>(updatedUser);

            return Ok(updatedUserDto);
        }

        [Authorize(Policy = "Privileged")]
        [HttpDelete("admin/DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser( int id )
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
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

        private string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secreteKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name, user.UserName)

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

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
