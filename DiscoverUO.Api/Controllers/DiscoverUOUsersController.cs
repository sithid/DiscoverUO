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
using DiscoverUO.Lib.Shared.Contracts;
using DiscoverUO.Lib.Shared;

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
        [HttpPost("authenticate")]
        public async Task<ActionResult<IResponse>> Authenticate(AuthenticationData loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == loginDto.Username);

            if (user == null)
            {
                var failedAuthorizationResponse = new RequestFailedResponse
                {
                    Success = false,
                    Message = "Invalid username or password.",
                    StatusCode = HttpStatusCode.Unauthorized,
                };

                return Unauthorized(failedAuthorizationResponse);
            }

            if(!string.Equals( loginDto.Password, user.PasswordHash ) )
            {
                var failedAuthorizationResponse = new AuthenticationResponse
                {
                    Success = false,
                    Message = "Invalid username or password.",
                    StatusCode = HttpStatusCode.Unauthorized,
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
        [HttpPost("register")]
        public async Task<ActionResult<IResponse>> RegisterUser(RegisterUserData registerUserData)
        {
            if (!ModelState.IsValid)
            {
                var registerUserResponse = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Invalid data!",
                };

                return BadRequest(registerUserResponse);
            }

            if ( string.IsNullOrEmpty( registerUserData.UserName ) || string.IsNullOrEmpty( registerUserData.Password ) )
            {
                var registerUserResponse = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "A valid username and password are required."
                };
            }

            var usernameToLower = registerUserData.UserName.ToLower();

            if( userNameExists( usernameToLower ) )
            {
                var registerUserResponse = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "That username already exists.  Please try something unique."
                };
            }

            var user = _mapper.Map<User>(registerUserData);


            user.PasswordHash = registerUserData.PasswordPreHashed ? registerUserData.Password : BCrypt.Net.BCrypt.HashPassword(registerUserData.Password);
            
            user.Role = UserRole.BasicUser;
            user.CreationDate = DateTime.Now.ToString();

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
                Entity = _mapper.Map<UserEntityData>(createdUser)
            };

            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createUserResponse );

        }

        [AllowAnonymous]
        [HttpGet("profiles/view/{id}")]
        public async Task<ActionResult<IResponse>> ViewProfileByID(int id)
        {
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(prof => prof.Id == id);

            if (userProfile == null)
            {
                var notFoundResponse = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "That user profile was not found!",
                };

                return NotFound(notFoundResponse);
            }

            var profileResponse = new ProfileResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.Created,
                Message = "User created successfully!",
                Entity = _mapper.Map<ProfileData>(userProfile)
            };

            return Ok(profileResponse);
        }

        #endregion

        #region BasicUser Endpoints

        [Authorize]
        [HttpGet("view/dashboard")]
        public async Task<ActionResult<IResponse>> GetDashboardData()
        {
            var user = await Permissions.GetCurrentUser(this.User, _context);

            if (user == null)
            {
                var notFound = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Your user data is missing!",
                };

                return NotFound(notFound);
            }

            Console.WriteLine($"Current User found: {user.UserName}, {user.Id}");

            var dashboardData = new DashboardData
            {
                Username = user.UserName,
                DailyVotesRemaining = user.DailyVotesRemaining,
                Email = user.Email,
                Role = user.Role.ToString(),
            };

            try
            {
                var prof = await _context.UserProfiles.FirstOrDefaultAsync(p => p.OwnerId == user.Id);

                if (prof != null && prof != default)
                {
                    dashboardData.UserBiography = prof.UserBiography;
                    dashboardData.UserAvatar = prof.UserAvatar;
                    dashboardData.UserDisplayName = prof.UserDisplayName;
                }
            }
            catch( Exception ex )
            {
                Console.WriteLine($"Something broke: {ex.Message}");
            }

            if ( user.Favorites != null )
            {
                dashboardData.Favorites = new FavoritesData();

                if( user.Favorites.FavoritedItems != null )
                {
                    dashboardData.Favorites.FavoritedItems = new List<FavoriteItemData>();

                    foreach( var fav in user.Favorites.FavoritedItems )
                    {
                        dashboardData.Favorites.FavoritedItems.Add(new FavoriteItemData
                        {
                            ServerName = fav.ServerName,
                            ServerAddress = fav.ServerAddress,
                            ServerPort = fav.ServerPort,
                            ServerEra = fav.ServerEra,
                            PvPEnabled = fav.PvPEnabled
                        });
                    }
                }
            }
            if (dashboardData != null)
            {
                var dashboardResponse = new DashboardDataResponse
                {
                    Success = true,
                    StatusCode = HttpStatusCode.OK,
                    Message = "Dashboard data located.",
                    Entity = dashboardData
                };

                return Ok(dashboardResponse);
            }
            else
            {
                var badRequest = new DashboardDataResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "The dashboard data associated with your user is missing!",
                };

                return BadRequest(badRequest);
            }
        }

        [Authorize] 
        [HttpGet("view/id/{id}")] 
        public async Task<ActionResult<IResponse>> GetUserById(int id)
        {
            var user = await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.Favorites)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                var notFound = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "That user does not exist.",
                };

                return NotFound(notFound);
            }

            var userRequest = _mapper.Map<UserEntityData>(user);

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
        [HttpGet("view/name/{userName}")] 
        public async Task<ActionResult<IResponse>> GetUserByName(string userName)
        {
            var user = await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.Favorites)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null)
            {
                var notFound = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "That user does not exist.",
                };

                return NotFound(notFound);
            }

            var userRequest = _mapper.Map<UserEntityData>(user);

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
        [HttpGet("profiles/view")]
        public async Task<ActionResult<IResponse>> GetProfile()
        {
            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                var userNoPerms = new RequestFailedResponse
                {
                    Success = false,
                    Message = "You must be logged in to do this.",
                    StatusCode = HttpStatusCode.Unauthorized,
                };

                return Unauthorized(userNoPerms);
            }

            var currentUserProfileRequest = _mapper.Map<ProfileData>(currentUser.Profile);

            var profileResponse = new ProfileResponse
            {
                Success = true,
                Message = $"User profile for {currentUserProfileRequest.UserDisplayName} found.",
                StatusCode = HttpStatusCode.OK,
                Entity = currentUserProfileRequest
            };

            return Ok(profileResponse);
        }

        [Authorize]
        [HttpPut("update/UpdateUser/{id}")] 
        public async Task<ActionResult<IResponse>> UpdateUserById(int userId, UpdateUserData updateUserRequest)
        {
            if (!ModelState.IsValid)
            {
                var badReqest = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = $"And then there was .... invalid data!",
                };

                return BadRequest(badReqest);
            }

            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                var userNoPerms = new RequestFailedResponse
                {
                    Success = false,
                    Message = "You must be logged in to do this.",
                    StatusCode = HttpStatusCode.Unauthorized,
                };

                return Unauthorized(userNoPerms);
            }

            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (targetUser == null)
            {
                var notFound = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "That user does not exist.",
                };

                return NotFound(notFound);
            }

            if (currentUser.Id != targetUser.Id)
            {
                if (!Permissions.HasElevatedRole(currentUser.Role) && !Permissions.HasHigherPermission(currentUser.Role, targetUser.Role))
                {
                    var userNoPerms = new RequestFailedResponse
                    {
                        Success = false,
                        Message = "You do not have permission to edit that user.",
                        StatusCode = HttpStatusCode.Unauthorized,
                    };

                    return Unauthorized(userNoPerms);
                }
            }

            targetUser.UserName = updateUserRequest.UserName;
            targetUser.Email = updateUserRequest.Email;

            _context.Entry(targetUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var badReqest = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = $"Something happened that noone was prepared for: {ex.Message}"
                };

                return BadRequest(badReqest);
            }

            var updatedUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            var updateUserResponse = new UserEntityResponse
            {
                Success = true,
                Message = "User data successfully updated.",
                StatusCode = HttpStatusCode.OK,
                Entity = _mapper.Map<UserEntityData>(updatedUser)
            };

            return Ok(updateUserResponse);
        }

        [Authorize]
        [HttpPut("update/UpdatePassword/{id}")] 
        public async Task<ActionResult<IResponse>> UpdatePassword(int userId, UpdateUserPasswordData updatePasswordRequest)
        {
            if (!ModelState.IsValid)
            {
                var badReqest = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = $"And then there was .... invalid data!",
                };

                return BadRequest(badReqest);
            }

            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                var userNoPerms = new RequestFailedResponse
                {
                    Success = false,
                    Message = "You do not have permission to edit that user.",
                    StatusCode = HttpStatusCode.Unauthorized,
                };

                return Unauthorized(userNoPerms);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                var userNotFound = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "See not, the user you seek.",
                };

                return NotFound(userNotFound);
            }

            if (currentUser.Id != user.Id)
            {
                if (!Permissions.HasElevatedRole(currentUser.Role) && !Permissions.HasHigherPermission(currentUser.Role, user.Role))
                {
                    var userNoPerms = new RequestFailedResponse
                    {
                        Success = false,
                        Message = "You do not have permission to edit that user.",
                        StatusCode = HttpStatusCode.Unauthorized,
                    };

                    return Unauthorized(userNoPerms);
                }
            }

            var userCurrentPassword = string.Empty;

            if (updatePasswordRequest.PasswordPreHashed)
                userCurrentPassword = updatePasswordRequest.CurrentPassword; // Password comes pre-hashed from the client.
            else
                userCurrentPassword = BCrypt.Net.BCrypt.HashPassword(updatePasswordRequest.CurrentPassword);

            if (!string.Equals( userCurrentPassword, user.PasswordHash))
            {
                var invalidPass = new RequestFailedResponse
                {
                    Success = false,
                    Message = "Invalid password.",
                    StatusCode = HttpStatusCode.Unauthorized,
                };

                return Unauthorized(invalidPass);
            }

            user.PasswordHash = updatePasswordRequest.PasswordPreHashed ?
                updatePasswordRequest.NewPassword : BCrypt.Net.BCrypt.HashPassword(updatePasswordRequest.NewPassword);

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var internalError = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"Something happened that no one was prepared for: {ex.Message}",
                };

                return BadRequest(internalError);
            }

            var updatedUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            var updatedUserResponse = new UserEntityResponse
            {
                Success = true,
                Message = $"Password updated successfully.",
                StatusCode = HttpStatusCode.OK,
                Entity = _mapper.Map<UserEntityData>(updatedUser)
            };

            return Ok(updatedUserResponse);
        }

        [Authorize]
        [HttpPut("profiles/update/UpdateProfile/{id}")] 
        public async Task<ActionResult<IResponse>> UpdateProfileById(int ownerId, ProfileData profileRequest)
        {
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(prof => prof.OwnerId == ownerId);

            if (userProfile == null)
            {
                var profileNotFound = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Now where did that profile go to...",
                };

                return NotFound(profileNotFound);
            }

            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser.Id != userProfile.OwnerId && !Permissions.HasElevatedRole(currentUser.Role))
            {
                var profileNoPerms = new RequestFailedResponse
                {
                    Success = false,
                    Message = "You are not authorized to update this profile.",
                    StatusCode = HttpStatusCode.Unauthorized,
                };

                return Unauthorized(profileNoPerms);
            }

            var profile = _mapper.Map<UserProfile>(profileRequest);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var badRequest = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"Something happened that no one was prepared for: {ex.Message}",
                };

                return BadRequest(badRequest);
            }

            var updatedProfile = await _context.UserProfiles
                .FirstOrDefaultAsync( prof => prof.OwnerId == ownerId);

            var profileResponse = new ProfileResponse
            {
                Success = true,
                Message = $"User profile for {updatedProfile.UserDisplayName} found.",
                StatusCode = HttpStatusCode.OK,
                Entity = _mapper.Map<ProfileData>(updatedProfile)
            };

            return Ok(profileResponse);
        }

        #endregion

        #region Privileged Endpoints

        [Authorize(Policy = "Privileged")] // Complete W/ GetUserEntityResponse
        [HttpPut("admin/update/votes/{id}")]
        public async Task<ActionResult<IResponse>> ChangeRemainingVotes(int userId, int votesRemaining)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                var userNotFound = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "That user was not found.",
                };

                return NotFound(userNotFound);
            }

            user.DailyVotesRemaining = votesRemaining;

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var badReqest = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = $"Something happened that noone was prepared for: {ex.Message}",
                };

                return BadRequest(badReqest);
            }

            var updatedUser = await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.Favorites)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var userResponse = new UserEntityResponse
            {
                Success = true,
                Message = "Votes remaining changed successfully.",
                StatusCode = HttpStatusCode.OK,
                Entity = _mapper.Map<UserEntityData>(updatedUser)
            };

            return Ok(userResponse);
        }

        [Authorize(Policy = "Privileged")]
        [HttpDelete("admin/DeleteUser/{id}")]
        public async Task<ActionResult<IResponse>> DeleteUser(int userId)
        {
            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                var failedAuthorizationResponse = new RequestFailedResponse
                {
                    Success = false,
                    Message = "You must be logged in to do this.",
                    StatusCode = HttpStatusCode.Unauthorized,
                };

                return Unauthorized(failedAuthorizationResponse);
            }

            var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == userId);

            if (user == null)
            {
                var notFoundResponse = new RequestFailedResponse
                {
                    Success = false,
                    Message = "Invalid username or password.",
                    StatusCode = HttpStatusCode.Unauthorized,
                };

                return NotFound(notFoundResponse);
            }

            if (currentUser.Id != user.Id)
            {
                if (!Permissions.HasElevatedRole(currentUser.Role))
                {
                    var noPerms = new RequestFailedResponse
                    {
                        Success = false,
                        Message = "You do not have permission to do that.",
                        StatusCode = HttpStatusCode.Unauthorized,
                    };

                    return Unauthorized(noPerms);
                }
            }

            if (!Permissions.HasHigherPermission(currentUser.Role, user.Role))
            {
                var lowMan = new RequestFailedResponse
                {
                    Success = false,
                    Message = "You do not have permission to do that.",
                    StatusCode = HttpStatusCode.Unauthorized,
                };

                return Unauthorized(lowMan);
            }

            var newServerOwner = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "Admin");

            if (newServerOwner == null)
            {
                newServerOwner = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "Owner");

                if (newServerOwner == null)
                {
                    var noOwnerFound = new RequestFailedResponse
                    {
                        Success = false,
                        Message = "No Suitable owner found to transfer ownership to.",
                        StatusCode = HttpStatusCode.InternalServerError,
                    };

                    return BadRequest(noOwnerFound);
                }
            }

            await _context.Servers
                .Where(s => s.OwnerId == userId)
                .ExecuteUpdateAsync(s => s.SetProperty(server => server.OwnerId, newServerOwner.Id));

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            var deleteSuccess = new BasicSuccessResponse
            {
                Success = true,
                Message = "The user has been deleted. No content exists.",
                StatusCode = HttpStatusCode.NoContent,
            };

            return Ok(deleteSuccess);
        }

        #endregion

        #region Admin Endpoints

        [Authorize(Policy = "OwnerOrAdmin")]
        [HttpGet("view/admin/all")]
        public async Task<ActionResult<IResponse>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.Favorites)
                .ToListAsync();

            if( users == null || users.Count == 0 )
            {
                var usersNotFound = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Apparently, all of our users grew legs and walked off...",
                };

                return NotFound(usersNotFound);
            }

            var allUsers = _mapper.Map<List<UserEntityData>>(users);


            var listResponse = new UserListResponse
            {
                Success = true,
                Message = "User data located.",
                StatusCode = HttpStatusCode.OK,
                List = allUsers
            };

            return Ok(listResponse);
        }

        [Authorize(Policy = "OwnerOrAdmin")]
        [HttpPut("admin/update/role/{id}")]
        public async Task<ActionResult<IResponse>> UpdateUserRole(int id, UserRole role)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == id);

            if (user == null)
            {
                var lostNNOTFound = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Seems weve lost it...",
                };
                return NotFound(lostNNOTFound);
            }

            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                var unauthorized = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.Unauthorized,
                    Message = "You do NOT have permission to do that!",
                };

                return Unauthorized(unauthorized);
            }

            if (currentUser.Id != user.Id && !Permissions.HasHigherPermission(currentUser.Role, role ) )
            {
                var unauthorized = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.Unauthorized,
                    Message = "You do NOT have permission to do that!",
                };

                return Unauthorized(unauthorized);
            }

            user.Role = role;

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var status500 = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"An error occurred while updating the server: {ex.Message}",
                };

                return BadRequest(status500);
            }

            var updatedUser = await _context.Users
                 .FirstOrDefaultAsync(u => u.Id == id);

            var userEntityData = new UserEntityResponse
            {
                Success = true,
                StatusCode = HttpStatusCode.OK,
                Message = $"User updated successfully.",
                Entity = _mapper.Map<UserEntityData>(updatedUser)
            };

            return Ok(userEntityData);
        }

        [Authorize(Policy = "OwnerOrAdmin")]
        [HttpPost("admin/register")]
        public async Task<ActionResult<IResponse>> RegisterUserWithRole(RegisterUserWithRoleData registerUserData)
        {
            if (!ModelState.IsValid)
            {
                var invalidModel = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Something went wrong!",
                };

                return BadRequest(invalidModel);
            }

            if (string.IsNullOrEmpty(registerUserData.UserName) || string.IsNullOrEmpty(registerUserData.Password))
            {
                var registerUserResponse = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "A valid username and password are required."
                };
            }

            var user = _mapper.Map<User>(registerUserData);

            if (registerUserData.PasswordPreHashed)
                user.PasswordHash = registerUserData.Password;
            else
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerUserData.Password);

            user.Role = registerUserData.Role;
            user.CreationDate = DateTime.Now.ToString();

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
                Entity = _mapper.Map<UserEntityData>(createdUser)
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

        private bool userNameExists(string userName)
        {
            var user = _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);

            if (user != null)
                return true;

            return false;
        }

        #endregion
    }
}
