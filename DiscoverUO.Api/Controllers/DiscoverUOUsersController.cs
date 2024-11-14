using AutoMapper;
using DiscoverUO.Api.Models;
using DiscoverUO.Lib.Shared.Users;
using DiscoverUO.Lib.Shared.Favorites;
using DiscoverUO.Lib.Shared.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
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

        }

        #region AllowAnonymous Endpoints

        [AllowAnonymous]
        [HttpPost("Authenticate")] // Complete W/ LoginResponse
        public async Task<ActionResult<AuthenticationResponse>> Authenticate(AuthenticationRequest loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.UserName == loginDto.Username);

            if (user == null)
            {
                var failedAuthorizationResponse = new AuthenticationResponse
                {
                    Success = false,
                    Message = "Invalid username or password.",
                    StatusCode = HttpStatusCode.Unauthorized,
                    Value = null
                };

                return Unauthorized(failedAuthorizationResponse);
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                var failedAuthorizationResponse = new AuthenticationResponse
                {
                    Success = false,
                    Message = "Invalid username or password.",
                    StatusCode = HttpStatusCode.Unauthorized,
                    Value = null
                };

                return Unauthorized(failedAuthorizationResponse);
            }

            var authResponse = new AuthenticationResponse
            {
                Success = true,
                Message = "You have been authenticated.",
                StatusCode = HttpStatusCode.OK,
                Value = GenerateToken(user)
            };

            return Ok(authResponse);
        }

        [AllowAnonymous]
        [HttpPost("RegisterUser")] // Complete W/ RegisterUserResponse
        public async Task<ActionResult<RegisterUserResponse>> RegisterUser(RegisterUserRequest registeredUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _mapper.Map<User>(registeredUserDto);

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registeredUserDto.Password);
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

            var createUserResponse = new RegisterUserResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.Created,
                Message = "User created successfully!",
                Entity = _mapper.Map<GetUserRequest>(createdUser)
            };

            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createUserResponse );

        }

        [AllowAnonymous]
        [HttpGet("profiles/view/{id}")]
        public async Task<ActionResult<GetProfileRequest>> ViewProfileByID(int id)
        {
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(prof => prof.Id == id);

            if (userProfile == null)
            {
                return NotFound();
            }

            var userProfileDto = _mapper.Map<GetProfileRequest>(userProfile);
            return Ok(userProfileDto);
        }

        #endregion

        #region BasicUser Endpoints

        [Authorize] // Complete W/ Response
        [HttpGet("view/dashboard")]
        public async Task<ActionResult<DashboardResponse>> GetDashboardData()
        {
            var user = await Permissions.GetCurrentUser(this.User, _context);

            System.Diagnostics.Debug.WriteLine($"DEBUG::User may be null.");

            if (user == null)
            {
                return NotFound("Your user is missing!");
            }

            System.Diagnostics.Debug.WriteLine($"DEBUG::User NOT null.");

            var dashboardData = new GetDashboardRequest
            {
                Username = user.UserName,
                DailyVotesRemaining = user.DailyVotesRemaining,
                Email = user.Email,
                Role = user.Role.ToString(),
                UserBiography = user.Profile.UserBiography,
                UserDisplayName = user.Profile.UserDisplayName,
                Favorites = _mapper.Map<GetFavoritesRequest>(user.Favorites),
            };

            if (dashboardData != null)
            {
                var dashboardResponse = new DashboardResponse
                {
                    Success = true,
                    StatusCode = HttpStatusCode.OK,
                    Message = "You eceived the dashboard data",
                    Entity = dashboardData
                };

                return Ok(dashboardResponse);
            }
            else
                return BadRequest("dashboardData is NULL");
        }

        [Authorize] 
        [HttpGet("view/id/{id}")] // Complete W/ UserResponse
        public async Task<ActionResult<UserEntityResponse>> GetUserById(int id)
        {
            var user = await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.Favorites)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var userRequest = _mapper.Map<GetUserRequest>(user);

            var userResponse = new UserEntityResponse
            {
                Success = true,
                Message = "User lookup successfull.",
                StatusCode = HttpStatusCode.OK,
                Entity = userRequest
            };

            return Ok(userResponse);
        }

        [Authorize]
        [HttpGet("view/name/{userName}")] // Complete W/ UserResponse
        public async Task<ActionResult<UserEntityResponse>> GetUserByName(string userName)
        {
            var user = await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.Favorites)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null)
            {
                return NotFound();
            }

            var userRequest = _mapper.Map<GetUserRequest>(user);

            var userResponse = new UserEntityResponse
            {
                Success = true,
                Message = "User lookup successfull.",
                StatusCode = HttpStatusCode.OK,
                Entity = userRequest
            };

            return Ok(userResponse);
        }

        [Authorize]
        [HttpGet("profiles/view")] // Add Customized Response
        public async Task<ActionResult<GetProfileRequest>> GetProfile()
        {
            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                return Unauthorized($"You must be logged in to do this.");
            }

            var userProfileDto = _mapper.Map<GetProfileRequest>(currentUser.Profile);
            return Ok(userProfileDto);
        }

        [Authorize]
        [HttpPut("update/UpdateUser/{id}")] // Add Customized Response
        public async Task<IActionResult> UpdateUser(int id, UpdateUserRequestRequest updateUserDto)
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

            var updatedUserDto = _mapper.Map<GetUserRequest>(updatedUser);

            return Ok(updatedUserDto);
        }

        [Authorize]
        [HttpPut("update/UpdatePassword/{id}")] // Add Customized Response
        public async Task<IActionResult> UpdatePassword(int id, UpdateUserPasswordRequest updatePasswordDto)
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

            var updatedUserDto = _mapper.Map<GetUserRequest>(updatedUser);

            return Ok(updatedUserDto);
        }

        [Authorize]
        [HttpPut("profiles/update/UpdateProfile/{id}")] // Add Customized Response
        public async Task<IActionResult> UpdateProfile(int id, GetProfileRequest profileDto)
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

            var updatedUserDto = _mapper.Map<GetUserRequest>(updatedUser);

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
        public async Task<ActionResult<UserTableResponse>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.Favorites)
                .ToListAsync();

            var allUsers = _mapper.Map<List<GetUserRequest>>(users);


            var tableResponse = new UserTableResponse
            {
                Success = true,
                Message = "User lookup successfull.",
                StatusCode = HttpStatusCode.OK,
                Table = allUsers
            };

            return Ok(tableResponse);
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

            if (!Permissions.HasHigherPermission(currentUser.Role, role))
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

            var updatedUserDto = _mapper.Map<GetUserRequest>(updatedUser);

            return Ok(updatedUserDto);
        }

        [Authorize(Policy = "OwnerOrAdmin")]
        [HttpPost("view/admin/CreateUserWithRole")] // Complete W/ RegisterUserResponse
        public async Task<ActionResult<RegisterUserResponse>> RegisterUserWithRole(RegisterUserWithRoleRequest registeredUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _mapper.Map<User>(registeredUserDto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registeredUserDto.Password);

            var userProfile = new UserProfile { OwnerId = user.Id, UserDisplayName = user.UserName };
            user.Profile = userProfile;

            var favoritesList = new UserFavoritesList { OwnerId = user.Id };
            user.Favorites = favoritesList;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var createdUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            var createUserResponse = new RegisterUserResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.Created,
                Message = "User created successfully!",
                Entity = _mapper.Map<GetUserRequest>(createdUser)
            };

            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createUserResponse);

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
