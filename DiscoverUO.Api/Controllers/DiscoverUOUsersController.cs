using AutoMapper;
using DiscoverUO.Api.Models;
using DiscoverUO.Lib.Shared.Users;
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
using DiscoverUO.Lib.Shared.Data;

namespace DiscoverUO.Api.Controllers
{
    [Route("api/users/")]
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

        [AllowAnonymous]  // IResponse
        [HttpPost("authenticate")]
        public async Task<ActionResult<IResponse>> Authenticate(AuthenticationData loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => string.Equals(u.UserName.ToLower(), loginDto.Username.ToLower()));

            if (user == null)
            {
                var failedAuthorizationResponse = new RequestFailedResponse
                {
                    Success = false,
                    Message = "Invalid username.",
                    StatusCode = HttpStatusCode.Unauthorized,
                };

                return Unauthorized(failedAuthorizationResponse);
            }

            if(!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash ) )
            {
                var failedAuthorizationResponse = new AuthenticationResponse
                {
                    Success = false,
                    Message = "Invalid password.",
                    StatusCode = HttpStatusCode.Unauthorized,
                };

                return Unauthorized(failedAuthorizationResponse);
            }

            var identityInfo = new IdentityData
            {
                Username = user.UserName.ToLower(),
                Role = user.Role,
                SecurityToken = GenerateToken(user)
            };

            var authResponse = new AuthenticationResponse
            {
                Success = true,
                Message = "You have been authenticated.",
                StatusCode = HttpStatusCode.OK,
                Entity = identityInfo
            };

            return Ok(authResponse);
        }

        [AllowAnonymous]   // IResponse
        [HttpPost("register")]
        public async Task<ActionResult> RegisterUser(RegisterUserData registerUserData)
        {
            if (!ModelState.IsValid)
            {
                var failedResponse = new RequestFailedResponse
                {
                    Success = false,
                    Message = "Invalid ModelState!",
                    StatusCode = HttpStatusCode.BadRequest
                };

                return BadRequest(failedResponse);
            }

            if ( string.IsNullOrEmpty( registerUserData.UserName.ToLower() ) || string.IsNullOrEmpty( registerUserData.Password ) )
            {
                var registerUserResponse = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "A valid username and password are required."
                };

                return BadRequest(registerUserResponse);
            }

            var usernameToLower = registerUserData.UserName.ToLower();

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerUserData.Password );
            
            if( userNameExists( usernameToLower, 0).Result )
            {
                var registerUserResponse = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "That username already exists.  Please try something unique."
                };

                return BadRequest(registerUserResponse);
            }

            var user = _mapper.Map<User>(registerUserData);

            user.UserName = registerUserData.UserName;
            user.Email = registerUserData.Email;
            user.PasswordHash = passwordHash;
            user.DailyVotesRemaining = 1;
            user.CreationDate = DateTime.Now.ToString();
            
            user.Role = UserRole.BasicUser;
            
            var userProfile = new UserProfile { OwnerId = user.Id, UserDisplayName = user.UserName.ToLower() };
            user.Profile = userProfile;

            var favoritesList = new UserFavoritesList { OwnerId = user.Id };
            user.Favorites = favoritesList;

            var favoritedServers = new List<UserFavoritesListItem>();
            user.Favorites.FavoritedItems = favoritedServers;

            _context.Users.Add(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var failedResponse = new RequestFailedResponse
                {
                    Success = false,
                    Message = $"An error occurred that nobody was prepared for: {ex.Message}",
                    StatusCode = HttpStatusCode.BadRequest
                };

                return BadRequest(failedResponse);
            }

            var createdUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser); 
        }

        [AllowAnonymous] // IResponse
        [HttpGet("profiles/view/id/{id}")]
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
                StatusCode = HttpStatusCode.OK,
                Message = "User profile found.",
                Entity = _mapper.Map<ProfileData>(userProfile)
            };

            return Ok(profileResponse);
        }

        [AllowAnonymous] // IResponse
        [HttpGet("profiles/view/{owner}")]
        public async Task<ActionResult<IResponse>> ViewProfileByDisplayName(string owner)
        {
            var profileOwner = await _context.Users.FirstOrDefaultAsync(user => string.Equals(user.UserName.ToLower(), owner));

            if( profileOwner == null)
            {
                var notFoundResponse = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "That user was not found."
                };

                return NotFound(notFoundResponse);
            }

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(prof => prof.OwnerId == profileOwner.Id);

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
                StatusCode = HttpStatusCode.OK,
                Message = "User profile found.",
                Entity = _mapper.Map<ProfileData>(userProfile)
            };

            return Ok(profileResponse);
        }

        #endregion

        #region BasicUser Endpoints

        [Authorize]  // IResponse
        [HttpGet("view")]
        public async Task<ActionResult<IResponse>> GetUserData()
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

            Console.WriteLine($"Current User found: {user.UserName.ToLower()}, {user.Id}");

            var userData = _mapper.Map<UserEntityData>(user);
            var userDataRsp = new UserEntityResponse
            {
                Success = true,
                Message = "User data found.",
                StatusCode = HttpStatusCode.OK,
                Entity = userData
            };

            return Ok(userDataRsp);
        }
        

        [Authorize] // IResponse
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

        [Authorize] // IResponse
        [HttpGet("view/name/{userName}")] 
        public async Task<ActionResult<IResponse>> GetUserByName(string userName)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => string.Equals( u.UserName.ToLower(),userName.ToLower()));

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

        [Authorize] // IResponse
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

            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(prof => prof.OwnerId == currentUser.Id);

            if (userProfile == null)
            {
                var failedNotFound = new RequestFailedResponse
                {
                    Success = false,
                    Message = "No profile found.",
                    StatusCode = HttpStatusCode.NotFound
                };

                return NotFound(failedNotFound);
            }

            var userProfileData = _mapper.Map<ProfileData>(userProfile);

            var profileResponse = new ProfileResponse
            {
                Success = true,
                Message = $"User profile for {currentUser.UserName.ToLower()} found.",
                StatusCode = HttpStatusCode.OK,
                Entity = userProfileData
            };

            return Ok(profileResponse);
        }

        [Authorize] // IResponse
        [HttpPut("update/UpdateUser/{id}")] 
        public async Task<ActionResult<IResponse>> UpdateUserById(int userId, UpdateUserData updateUserRequest)
        {
            if (!ModelState.IsValid)
            {
                var failedResponse = new RequestFailedResponse
                {
                    Success = false,
                    Message = "Invalid ModelState!",
                    StatusCode = HttpStatusCode.BadRequest
                };

                return BadRequest(failedResponse);
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

            if( userNameExists(updateUserRequest.UserName.ToLower(), userId).Result )
            {
                var userBadRequest = new RequestFailedResponse
                {
                    Success = false,
                    Message = "That username is in use.",
                    StatusCode = HttpStatusCode.BadRequest,
                };

                return Unauthorized(userBadRequest);
            }

            targetUser.UserName = updateUserRequest.UserName.ToLower();
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

        [Authorize] // IResponse
        [HttpPut("updateuser/{username}")]
        public async Task<ActionResult<IResponse>> UpdateUserByName(string username, UpdateUserData updateUserRequest)
        {
            if (!ModelState.IsValid)
            {
                var failedResponse = new RequestFailedResponse
                {
                    Success = false,
                    Message = "Invalid ModelState!",
                    StatusCode = HttpStatusCode.BadRequest
                };

                return BadRequest(failedResponse);
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

            var targetUser = await _context.Users.FirstOrDefaultAsync(u => string.Equals(u.UserName.ToLower(), username.ToLower()));

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

            if (currentUser.UserName.ToLower() != username.ToLower())
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

            if (userNameExists(updateUserRequest.UserName.ToLower(), targetUser.Id).Result)
            {
                var userBadRequest = new RequestFailedResponse
                {
                    Success = false,
                    Message = "That username is in use.",
                    StatusCode = HttpStatusCode.BadRequest,
                };

                return Unauthorized(userBadRequest);
            }

            targetUser.UserName = updateUserRequest.UserName.ToLower();
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
                .FirstOrDefaultAsync(u => string.Equals( u.UserName.ToLower(), updateUserRequest.UserName.ToLower()));

            var updateUserResponse = new UserEntityResponse
            {
                Success = true,
                Message = "User data successfully updated.",
                StatusCode = HttpStatusCode.OK,
                Entity = _mapper.Map<UserEntityData>(updatedUser)
            };

            return Ok(updateUserResponse);
        }


        [Authorize] // IResponse
        [HttpPut("password/update/id/{id}")] 
        public async Task<ActionResult<IResponse>> UpdatePassword(int userId, UpdateUserPasswordData updatePasswordRequest)
        {
            if (!ModelState.IsValid)
            {
                var failedResponse = new RequestFailedResponse
                {
                    Success = false,
                    Message = "Invalid ModelState!",
                    StatusCode = HttpStatusCode.BadRequest
                };

                return BadRequest(failedResponse);
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


            var userCurrentPassword = BCrypt.Net.BCrypt.HashPassword(updatePasswordRequest.CurrentPassword);

            if (!BCrypt.Net.BCrypt.Verify(updatePasswordRequest.CurrentPassword, user.PasswordHash))
            {
                var invalidPass = new RequestFailedResponse
                {
                    Success = false,
                    Message = "Invalid password.",
                    StatusCode = HttpStatusCode.Unauthorized,
                };

                return Unauthorized(invalidPass);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updatePasswordRequest.NewPassword);

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
                    Message = $"Something happened that no one was prepared for: {ex.Message}",
                    StatusCode = HttpStatusCode.BadRequest,
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

        [Authorize] // IResponse
        [HttpPut("password/update/{username}")]
        public async Task<ActionResult<IResponse>> UpdatePasswordByName(string username, UpdateUserPasswordData updatePasswordRequest)
        {
            if (!ModelState.IsValid)
            {
                var failedResponse = new RequestFailedResponse
                {
                    Success = false,
                    Message = "Invalid ModelState!",
                    StatusCode = HttpStatusCode.BadRequest
                };

                return BadRequest(failedResponse);
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

            var user = await _context.Users.FirstOrDefaultAsync(u => string.Equals( u.UserName.ToLower(), username.ToLower()));

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

            var userCurrentPassword = BCrypt.Net.BCrypt.HashPassword(updatePasswordRequest.CurrentPassword);

            if (!BCrypt.Net.BCrypt.Verify(updatePasswordRequest.CurrentPassword, user.PasswordHash))
            {
                var invalidPass = new RequestFailedResponse
                {
                    Success = false,
                    Message = "Invalid password.",
                    StatusCode = HttpStatusCode.Unauthorized,
                };

                return Unauthorized(invalidPass);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updatePasswordRequest.NewPassword);

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
                    Message = $"Something happened that no one was prepared for: {ex.Message}",
                    StatusCode = HttpStatusCode.BadRequest,
                };

                return BadRequest(internalError);
            }

            var updatedUser = await _context.Users
                .FirstOrDefaultAsync(u => string.Equals(u.UserName.ToLower(), username.ToLower()));

            var updatedUserResponse = new UserEntityResponse
            {
                Success = true,
                Message = $"Password updated successfully.",
                StatusCode = HttpStatusCode.OK,
                Entity = _mapper.Map<UserEntityData>(updatedUser)
            };

            return Ok(updatedUserResponse);
        }


        [Authorize] // IResponse
        [HttpPut("profiles/update/{profId}")] 
        public async Task<ActionResult<IResponse>> UpdateProfileById(int profId, ProfileData profileRequest)
        {
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(prof => prof.Id == profId);

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

            userProfile.UserDisplayName = profile.UserDisplayName;
            userProfile.UserAvatar = profile.UserAvatar;
            userProfile.UserBiography = profile.UserBiography;

            _context.Entry( userProfile ).State = EntityState.Modified;

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
                .FirstOrDefaultAsync( prof => prof.Id == profId);

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

        [Authorize(Policy = "Privileged")] // IResponse
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
                    Message = $"Something happened that noone was prepared for: {ex.Message}",
                    StatusCode = HttpStatusCode.BadRequest
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

        [Authorize(Policy = "Privileged")] // IResponse
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
                        StatusCode = HttpStatusCode.BadRequest,
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

        [Authorize(Policy = "OwnerOrAdmin")] // IResponse
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

        [Authorize(Policy = "OwnerOrAdmin")] // IResponse
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
                var internalServerError = new RequestFailedResponse
                {
                    Success = false,
                    Message = $"An error occurred while updating the server: {ex.Message}",
                    StatusCode = HttpStatusCode.BadRequest,
                };

                return BadRequest(internalServerError);
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

        [Authorize(Policy = "OwnerOrAdmin")] // IResponse
        [HttpPost("admin/register")]
        public async Task<ActionResult> RegisterUserWithRole(RegisterUserWithRoleData registerUserData)
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

            user.UserName = registerUserData.UserName;
            user.Email = registerUserData.Email;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerUserData.Password);
            user.DailyVotesRemaining = 1;
            user.CreationDate = DateTime.Now.ToString();
            
            user.Role = registerUserData.Role;

            var userProfile = new UserProfile { OwnerId = user.Id, UserDisplayName = user.UserName };
            user.Profile = userProfile;

            var favoritesList = new UserFavoritesList { OwnerId = user.Id };
            user.Favorites = favoritesList;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var createdUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            var createdUserData = _mapper.Map<UserEntityData>(createdUser);

            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUserData);
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

        private async Task<bool> userNameExists(string userName, int userId )
        {

            var user = await _context.Users.FirstOrDefaultAsync(u => string.Equals( u.UserName.ToLower(), userName));

            if (user != null && user.Id != userId )
                return true;

            return false;
        }

        #endregion
    }
}
