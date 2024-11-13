using AutoMapper;
using DiscoverUO.Api.Models;
using DiscoverUO.Lib.DTOs;
using DiscoverUO.Lib.DTOs.Profiles;
using DiscoverUO.Lib.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

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

        }

        #region AllowAnonymous Endpoints

        [AllowAnonymous]
        [HttpPost("Authenticate")]
        public async Task<IActionResult> Authenticate(LoginRequest loginDto)
        {
            Console.WriteLine($"\n\rAuthenticate::LoginRequest({loginDto.Username}, *****)");

            var user = await _context.Users.SingleOrDefaultAsync(u => u.UserName == loginDto.Username);

            if (user == null)
            {
                Console.WriteLine($"\n\rAuthenticate::LoginRequest::Failed, Invalid Username: {loginDto.Username}");
                return Unauthorized("Invalid username.");
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                Console.WriteLine($"\n\rAuthenticate::LoginRequest::Failed, Invalid Password for: {user.UserName} ");
                return Unauthorized("Invalid password.");
            }

            Console.WriteLine($"\n\rAuthenticate::LoginRequest::Success: {user.UserName}[{user.Role}]");

            var token = GenerateToken(user);

            return Ok(token);
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
            user.Role = UserRole.BasicUser;

            var userProfile = new UserProfile { OwnerId = user.Id, UserDisplayName = user.UserName };

            user.Profile = userProfile;

            var favoritesList = new UserFavoritesList { OwnerId = user.Id };
            user.Favorites = favoritesList;

            var favoritedServers = new List<UserFavoritesListItem>();
            user.Favorites.FavoritedItems = favoritedServers;

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            var createdUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            var userDto = _mapper.Map<User>(createdUser);

            return CreatedAtAction("GetUserById", new { id = createdUser.Id }, userDto);

        }

        [AllowAnonymous]
        [HttpGet("profiles/view/{id}")]
        public async Task<ActionResult<UserProfileDto>> ViewProfileByID(int id)
        {
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(prof => prof.Id == id);

            if (userProfile == null)
            {
                return NotFound();
            }

            var userProfileDto = _mapper.Map<UserProfileDto>(userProfile);
            return Ok(userProfileDto);
        }

        #endregion

        #region BasicUser Endpoints

        [Authorize]
        [HttpGet("view/dashboard")]
        public async Task<ActionResult<DashboardDto>> GetDashboardData()
        {
            var user = await Permissions.GetCurrentUser(this.User, _context);

            System.Diagnostics.Debug.WriteLine($"DEBUG::User may be null.");

            if (user == null)
            {
                return NotFound("Your user is missing!");
            }

            System.Diagnostics.Debug.WriteLine($"DEBUG::User NOT null.");

            var dashboardData = new DashboardDto
            {
                Username = user.UserName,
                DailyVotesRemaining = user.DailyVotesRemaining,
                Email = user.Email,
                Role = user.Role.ToString(),
                UserBiography = user.Profile.UserBiography,
                UserDisplayName = user.Profile.UserDisplayName,
            };

            if (dashboardData != null)
            {
                var data = JsonSerializer.Serialize(dashboardData);

                HttpContext.Response.Headers.ContentType = "application/json";
                return Ok(data);
            }
            else
                return BadRequest("dashboardData is NULL");
        }

        [Authorize]
        [HttpGet("view/id/{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            var user = await _context.Users
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
        [HttpGet("view/name/{userName}")]
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
        [HttpGet("profiles/view")]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                return Unauthorized($"You must be logged in to do this.");
            }

            var userProfileDto = _mapper.Map<UserProfileDto>(currentUser.Profile);
            return Ok(userProfileDto);
        }

        [Authorize]
        [HttpPut("update/UpdateUser/{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

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
                if (!Permissions.HasElevatedRole(currentUser.Role) && !Permissions.HasHigherPermission(currentUser.Role, user.Role))
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
        [HttpPut("update/UpdatePassword/{id}")]
        public async Task<IActionResult> UpdatePassword(int id, UpdateUserPasswordDto updatePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

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
                if (!Permissions.HasElevatedRole(currentUser.Role) && !Permissions.HasHigherPermission(currentUser.Role, user.Role))
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

        [Authorize]
        [HttpPut("profiles/update/UpdateProfile/{id}")]
        public async Task<IActionResult> UpdateProfile(int id, UserProfileDto profileDto)
        {
            var userProfile = await _context.UserProfiles.FindAsync(id);

            if (userProfile == null)
            {
                return NotFound();
            }

            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser.Id != userProfile.OwnerId && !Permissions.HasElevatedRole(currentUser.Role))
            {
                return Unauthorized("You are not authorized to update this profile.");
            }

            var profile = _mapper.Map<UserProfile>(profileDto);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return BadRequest($"Something happened that no one was prepared for: {ex}");
            }

            return NoContent();
        }

        #endregion

        #region Privileged Endpoints

        [Authorize(Policy = "Privileged")]
        [HttpPut("admin/update/ChangeRemainingVotes/{id}")]
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
            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

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
                if (!Permissions.HasElevatedRole(currentUser.Role))
                {
                    return Unauthorized("You do not have permission to do that.");
                }
            }

            if (!Permissions.HasHigherPermission(currentUser.Role, user.Role))
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
        [HttpGet("view/admin/All")]
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
        [HttpPut("view/admin/UpdateUserRole/{id}")]
        public async Task<IActionResult> UpdateUserRole(int id, UserRole role)
        {
            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

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
                if (!Permissions.HasElevatedRole(currentUser.Role))
                {
                    return Unauthorized("You do not have permission to do that.");
                }
            }

            if( !Permissions.HasHigherPermission( currentUser.Role, role ))
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

        [Authorize(Policy = "OwnerOrAdmin")]
        [HttpPost("view/admin/CreateUserWithRole")]
        public async Task<ActionResult<UserDto>> CreateUserWithRole(CreateUserWithRoleDto createdUserDto)
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

            var userDto = _mapper.Map<UserDto>(createdUser);

            return CreatedAtAction("GetUserById", new { id = createdUser.Id }, userDto);

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
                new Claim(ClaimTypes.Role, requester.Role.ToString()),
                new Claim(ClaimTypes.Name, requester.UserName)

            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)

            };

            var tokenString = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(tokenString);
        }

        #endregion
    }
}
