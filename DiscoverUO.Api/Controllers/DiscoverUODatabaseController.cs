using DiscoverUO.Api.Models;
using DiscoverUO.Lib.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DiscoverUO.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscoverUODatabaseController : ControllerBase
    {
        private readonly DiscoverUODatabaseContext _context;
        private readonly string secreteKey;

        public DiscoverUODatabaseController(DiscoverUODatabaseContext context)
        {
            _context = context;

            secreteKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

            if (string.IsNullOrEmpty(secreteKey))
            {
                secreteKey = "temp_secret_key_for_initial_setup";
                Console.WriteLine("JWT secret key not found in environment variables.");
                Console.WriteLine("Using a temporary key.  Fix this immediately.");
            }
        }

        [AllowAnonymous]
        [HttpPost("Login")]
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
        [HttpGet("GetAllUsers")]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers()
        {
            var currentUsers = await _context.Users.ToListAsync();
            var currentUsersDtos = new List<UserDto>();

            foreach( var user in currentUsers)
            {
                var detailedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

                if (detailedUser == null)
                {
                    return NotFound();
                }

                var userDto = new UserDto
                {
                    UserName = detailedUser.UserName,
                    Role = detailedUser.Role,
                    DailyVotesRemaining = detailedUser.DailyVotesRemaining,
                    ServersAddedIds = detailedUser.ServersAdded?.Select(s => s.Id).ToList(),
                    ProfileId = detailedUser.Profile?.Id,
                    FavoritesId = detailedUser.Favorites?.Id
                };

                currentUsersDtos.Add(userDto);
            }

            return Ok(currentUsersDtos);
        }

        [Authorize(Policy = "BasicUser")]
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
                UserName = user.UserName,
                DailyVotesRemaining = user.DailyVotesRemaining,
                ServersAddedIds = user.ServersAdded?.Select(s => s.Id).ToList(),
                ProfileId = user.Profile?.Id,
                FavoritesId = user.Favorites?.Id
            };

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

            var user = new User
            {
                UserName = createUserDto.UserName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
                Role = "BasicUser"
            };

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

            var userDto = new UserDto
            {
                UserName = createdUser.UserName,
                Role = createdUser.Role,
                DailyVotesRemaining = createdUser.DailyVotesRemaining,
                ServersAddedIds = createdUser.ServersAdded?.Select(s => s.Id).ToList(),
                ProfileId = createdUser.Profile?.Id,
                FavoritesId = createdUser.Favorites?.Id,
            };

            return CreatedAtAction(nameof(GetUser), new { id = createdUser }, userDto);

        }

        [Authorize(Policy = "OwnerOrAdmin")]
        [HttpPost("CreateUserWithRole")]
        public async Task<ActionResult<UserDto>> CreateUserWithRole(CreateUserDto createdUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new User
            {
                UserName = createdUserDto.UserName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(createdUserDto.Password),
                Role = createdUserDto.Role
            };

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

            var userDto = new UserDto
            {
                UserName = createdUser.UserName,
                DailyVotesRemaining = createdUser.DailyVotesRemaining,
                ServersAddedIds = createdUser.ServersAdded?.Select(s => s.Id).ToList(),
                ProfileId = createdUser.Profile?.Id,
                FavoritesId = createdUser.Favorites?.Id,
            };

            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, userDto);

        }

        [Authorize]
        [HttpPut("UpdateUser/{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto userDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            user.UserName = userDto.UserName;
            user.Email = userDto.Email;

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

            var updatedUserDto = new UserDto
            {
                UserName = updatedUser.UserName,
                DailyVotesRemaining = updatedUser.DailyVotesRemaining,
                Role = updatedUser.Role,
                Email = updatedUser.Email,
                ServersAddedIds = updatedUser.ServersAdded.Select(s => s.Id).ToList(),
                ProfileId = updatedUser.Profile?.Id,
                FavoritesId = updatedUser.Favorites?.Id
            };

            return Ok(updatedUserDto);
        }

        [Authorize]
        [HttpPut("UpdateUserPassword/{id}")]
        public async Task<IActionResult> UpdateUserPassword(int id, UpdateUserPasswordDto userDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            if (!BCrypt.Net.BCrypt.Verify(userDto.CurrentPassword, user.PasswordHash))
            {
                return Unauthorized("Invalid password.");
            }


            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.NewPassword);

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

            var updatedUserDto = new UserDto
            {
                UserName = updatedUser.UserName,
                DailyVotesRemaining = updatedUser.DailyVotesRemaining,
                ServersAddedIds = updatedUser.ServersAdded.Select(s => s.Id).ToList(),
                ProfileId = updatedUser.Profile?.Id,
                FavoritesId = updatedUser.Favorites?.Id
            };

            return Ok(updatedUserDto);
        }

        [Authorize(Policy = "OwnerOrAdmin")]
        [HttpPut("UpdateUserRole/{id}")]
        public async Task<IActionResult> UpdateUserRole(int id, UpdateUserRoleDto roleDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            user.Role = roleDto.Role;

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

            var updatedUserDto = new UserDto
            {
                UserName = updatedUser.UserName,
                DailyVotesRemaining = updatedUser.DailyVotesRemaining,
                Role = updatedUser.Role,
                Email = updatedUser.Email,
                ServersAddedIds = updatedUser.ServersAdded.Select(s => s.Id).ToList(),
                ProfileId = updatedUser.Profile?.Id,
                FavoritesId = updatedUser.Favorites?.Id
            };

            return Ok(updatedUserDto);
        }

        [Authorize]
        [HttpPut("UpdateUserVotesRemaining/{id}")]
        public async Task<IActionResult> UpdateUserVotesRemaining(int id, UpdateUserVotesRemainingDto votesDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            user.DailyVotesRemaining = votesDto.DailyVotesRemaining;

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

            var updatedUserDto = new UserDto
            {
                UserName = updatedUser.UserName,
                DailyVotesRemaining = updatedUser.DailyVotesRemaining,
                Role = updatedUser.Role,
                Email = updatedUser.Email,
                ServersAddedIds = updatedUser.ServersAdded.Select(s => s.Id).ToList(),
                ProfileId = updatedUser.Profile?.Id,
                FavoritesId = updatedUser.Favorites?.Id
            };

            return Ok(updatedUserDto);
        }

        [Authorize(Policy = "Privileged")]
        [HttpDelete("DeleteUser/{userId}")]
        public async Task<IActionResult> DeleteUser( int userId )
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            // Make sure we transfer any orphaned servers to a new owner to maintain data.

            var newServerOwner = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "admin");

            if (newServerOwner == null)
            {
                newServerOwner = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "owner");

                if (newServerOwner == null)
                {
                    return BadRequest();
                }
            }

            var serversToTransfer = await _context.Servers.Where(s => s.OwnerId == userId).ToListAsync();

            foreach (var server in serversToTransfer)
            {
                server.OwnerId = newServerOwner.Id;
            }
            await _context.SaveChangesAsync();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secreteKey); // temp until I decide if I want to stick with how I am doing login.

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
