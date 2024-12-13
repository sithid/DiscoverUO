using AutoMapper;
using DiscoverUO.Api.Models;
using DiscoverUO.Lib.Shared;
using DiscoverUO.Lib.Shared.Contracts;
using DiscoverUO.Lib.Shared.Servers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace DiscoverUO.Api.Controllers
{
    [Route("api/servers")]
    [ApiController]
    public class DiscoverUOServersController : ControllerBase
    {
        #region Private Fields

        private readonly DiscoverUODatabaseContext _context;
        private readonly IMapper _mapper;

        #endregion

        public DiscoverUOServersController(DiscoverUODatabaseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        #region Anonymous+ Endpoints

        [AllowAnonymous]
        [HttpGet("public")]  // IResponse
        public async Task<ActionResult<IResponse>> GetServers()
        {
            var servers = await _context.Servers.ToListAsync();

            if( servers == null || servers.Count <= 0 )
            {
                var failedNotFound = new RequestFailedResponse()
                {
                    Success = false,
                    Message = "No public servers found.",
                    StatusCode = HttpStatusCode.NotFound
                };

                return NotFound(failedNotFound);
            }
            
            var serverListData = _mapper.Map<List<ServerData>>(servers);

            var serverDataResponse = new ServerListDataResponse
            {
                Success = true,
                Message = "Serverlist found.",
                StatusCode = System.Net.HttpStatusCode.OK,
                List = serverListData
            };

            return Ok(serverDataResponse);
        }

        [AllowAnonymous]  // IResponse
        [HttpGet("view/id/{id}")]
        public async Task<ActionResult<IResponse>> GetServerById(int id)
        {
            var server = await _context.Servers
                .FirstOrDefaultAsync(server => server.Id == id);

            if (server == null)
            {
                var response = new RequestFailedResponse
                {
                    Success = false,
                    Message = "Server not found.",
                    StatusCode = HttpStatusCode.NotFound
                };

                return NotFound(response);
            }

            var serverData = _mapper.Map<ServerData>(server);

            var serverDataResponse = new ServerDataResponse
            {
                Success = true,
                Message = "Serverlist found.",
                StatusCode = System.Net.HttpStatusCode.OK,
                Entity = serverData
            };

            return Ok(serverDataResponse);
        }

        [AllowAnonymous] // IResponse
        [HttpGet("view/name/{serverName}")]
        public async Task<ActionResult<IResponse>> GetServerByName(string serverName)
        {
            var server = await _context.Servers
                .FirstOrDefaultAsync(s => string.Equals(s.ServerName, serverName));

            if (server == null)
            {
                var failedNotFound = new RequestFailedResponse
                {
                    Success = false,
                    Message = "Server not found.",
                    StatusCode = HttpStatusCode.NotFound
                };

                return NotFound(failedNotFound);
            }

            var serverData = _mapper.Map<ServerData>(server);

            var serverDataResponse = new ServerDataResponse
            {
                Success = true,
                Message = $"Server {serverName} found.",
                StatusCode = HttpStatusCode.OK,
                Entity = serverData
            };

            return Ok(serverDataResponse);
        }

        [AllowAnonymous]  // IResponse
        [HttpGet("view/owner/{userName}")]
        public async Task<ActionResult<IResponse>> FindServersByOwner(string userName)
        {
            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                var failedUnauthorized = new RequestFailedResponse
                {
                    Success = false,
                    Message = "You must be logged in to do this.",
                    StatusCode = HttpStatusCode.Unauthorized
                };

                return Unauthorized(failedUnauthorized);
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => string.Equals(u.UserName, userName));

            if (existingUser == null)
            {
                var failedNotFound = new RequestFailedResponse
                {
                    Success = false,
                    Message = "The user does not exist.",
                    StatusCode = HttpStatusCode.Unauthorized
                };

                return NotFound(failedNotFound);
            }

            var ownedServers = await _context.Servers
                .Where(server => server.OwnerId == existingUser.Id)
                .ToListAsync();

            if (ownedServers == null)
            {
                var failedNotFound = new RequestFailedResponse
                {
                    Success = false,
                    Message = "That user doesnt own any servers.",
                    StatusCode = HttpStatusCode.Unauthorized
                };

                return NotFound(failedNotFound);
            }

            var ownedServersData = _mapper.Map<List<ServerData>>(ownedServers);

            var ownedServersResponse = new ServerListDataResponse
            {
                Success = true,
                Message = $"{ownedServers.Count} servers found.",
                StatusCode = HttpStatusCode.OK,
                List = ownedServersData
            };

            return Ok(ownedServersResponse);
        }

        #endregion

        #region BasicUser+ Endpoints

        [Authorize] // IResponse
        [HttpGet("view/owned")]
        public async Task<ActionResult<IResponse>> GetOwnedServers()
        {
            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                var failedUnauthorized = new RequestFailedResponse
                {
                    Success = false,
                    Message = "You must be logged in to do this.",
                    StatusCode = HttpStatusCode.Unauthorized
                };

                return Unauthorized(failedUnauthorized);
            }

            var ownedServers = await _context.Servers
                .Where(server => server.OwnerId == currentUser.Id)
                .ToListAsync();

            if (ownedServers == null)
            {
                var failedNotFound = new RequestFailedResponse
                {
                    Success = false,
                    Message = "You do not own any servers.",
                    StatusCode = HttpStatusCode.Unauthorized
                };

                return NotFound(failedNotFound);
            }

            var ownedServersData = _mapper.Map<List<ServerData>>(ownedServers);

            var ownedServersResponse = new ServerListDataResponse
            {
                Success = true,
                Message = $"{ownedServers.Count} servers found.",
                StatusCode = HttpStatusCode.OK,
                List = ownedServersData
            };

            return Ok(ownedServersResponse);
        }

        [Authorize] // IResponse
        [HttpPost("create_server")]
        public async Task<ActionResult> AddServer(ServerRegistrationData createServerData)
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
                var failedUnauthorized = new RequestFailedResponse
                {
                    Success = false,
                    Message = "You must be logged in to do this.",
                    StatusCode = HttpStatusCode.Unauthorized
                };

                return Unauthorized(failedUnauthorized);
            }

            if( AddressInUse( createServerData.ServerAddress ).Result )
            {
                var createServerRsp = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "That server address is already in use.  Please try something unique."
                };

                return BadRequest(createServerRsp);
            }

            var serverToAdd = _mapper.Map<Server>(createServerData);
            serverToAdd.OwnerId = currentUser.Id;

            _context.Servers.Add(serverToAdd);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var failedResponse = new RequestFailedResponse
                {
                    Success = false,
                    Message = $"An error occurred while adding the server: {ex.Message}",
                    StatusCode = HttpStatusCode.Unauthorized
                };

                return BadRequest(failedResponse);
            }

            var createdServer = await _context.Servers
                .FirstOrDefaultAsync(s => s.Id == serverToAdd.Id);

            var createdServerData = _mapper.Map<ServerData>(createdServer);

            return CreatedAtAction(nameof(GetServerById), new { id = createdServer.Id }, createdServerData);
        }

        [Authorize] // IResponse
        [HttpPut("updateserver/{serverId}")]
        public async Task<ActionResult<IResponse>> UpdateServer(int serverId, ServerUpdateData serverUpdateData)
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
                var failedUnauthorized = new RequestFailedResponse
                {
                    Success = false,
                    Message = "You must be logged in to do this.",
                    StatusCode = HttpStatusCode.Unauthorized
                };

                return Unauthorized(failedUnauthorized);
            }

            var serverToUpdate = await _context.Servers.FirstOrDefaultAsync(server => server.Id == serverId);

            if (serverToUpdate == null)
            {
                var failedNotFound = new RequestFailedResponse
                {
                    Success = false,
                    Message = "Server not found.",
                    StatusCode = HttpStatusCode.NotFound
                };

                return NotFound(failedNotFound);
            }

            if (!Permissions.HasServerPermissions(currentUser, serverToUpdate))
            {
                var failedUnauthorized = new RequestFailedResponse
                {
                    Success = false,
                    Message = "Only the owner of a server or a privileged user can update a servers information.",
                    StatusCode = HttpStatusCode.Unauthorized
                };

                return Unauthorized(failedUnauthorized);
            }

            serverToUpdate.ServerName = serverUpdateData.ServerName;
            serverToUpdate.ServerAddress = serverUpdateData.ServerAddress;
            serverToUpdate.ServerPort = serverUpdateData.ServerPort;
            serverToUpdate.ServerEra = serverUpdateData.ServerEra;
            serverToUpdate.ServerWebsite = serverUpdateData.ServerWebsite;
            serverToUpdate.ServerBanner = serverUpdateData.ServerBanner;
            serverToUpdate.PvPEnabled = serverUpdateData.PvPEnabled;
            serverToUpdate.IsPublic = serverUpdateData.IsPublic;

            _context.Entry(serverToUpdate).State = EntityState.Modified;

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

            var updatedServer = await _context.Servers
                .FirstOrDefaultAsync(s => s.Id == serverToUpdate.Id);

            var updatedServerData = _mapper.Map<ServerData>(updatedServer);

            var updatedServerResponse = new ServerDataResponse
            {
                Success = true,
                Message = "Server created successfully.",
                StatusCode = HttpStatusCode.OK,
                Entity = updatedServerData
            };

            return Ok(updatedServerResponse);
        }

        [Authorize] // IResponse
        [HttpPut("ownership/transfer/{serverId}_{newOwnerName}")]
        public async Task<ActionResult<IResponse>> TransferOwnership(int serverId, string newOwnerName )
        {
            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                var failedUnauthorized = new RequestFailedResponse
                {
                    Success = false,
                    Message = "You must be logged in to do this.",
                    StatusCode = HttpStatusCode.Unauthorized
                };

                return Unauthorized(failedUnauthorized);
            }

            var server = await _context.Servers.FirstOrDefaultAsync(s => s.Id == serverId);

            if (server == null)
            {
                var failedNotFound = new RequestFailedResponse
                {
                    Success = false,
                    Message = "Server not found.",
                    StatusCode = HttpStatusCode.NotFound
                };

                return NotFound(failedNotFound);
            }

            if (!Permissions.HasServerPermissions(currentUser, server))
            {
                var failedUnauthorized = new RequestFailedResponse
                {
                    Success = false,
                    Message = "Only the owner of a server or a privileged user can transfer server ownership.",
                    StatusCode = HttpStatusCode.Unauthorized
                };

                return Unauthorized(failedUnauthorized);
            }

            var newOwner = await _context.Users.FirstOrDefaultAsync(user => string.Equals(user.UserName, newOwnerName));

            if (newOwner == null)
            {
                var ownerNotFound = new RequestFailedResponse
                {
                    Success = false,
                    Message = "New owner not found.",
                    StatusCode = HttpStatusCode.NotFound
                };

                return NotFound(ownerNotFound);
            }

            server.OwnerId = newOwner.Id;

            _context.Entry(server).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var badRequest = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"Something happened that no one was prepared for: {ex.Message}",
                };

                return BadRequest(badRequest);
            }

            var updatedServer = await _context.Servers
                .FirstOrDefaultAsync(server => server.Id == serverId);

            var serverData = _mapper.Map<ServerData>(updatedServer);

            var serverDataResponse = new ServerDataResponse
            {
                Success = true,
                Message = "Ownership transfered.",
                StatusCode = HttpStatusCode.OK,
                Entity = serverData
            };

            return Ok(serverDataResponse);
        }

        [Authorize]  // IResponse
        [HttpDelete("delete/{serverId}")]
        public async Task<ActionResult<IResponse>> DeleteServer(int serverId)
        {
            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                var failedUnauthorized = new RequestFailedResponse
                {
                    Success = false,
                    Message = "You must be logged in to do this.",
                    StatusCode = HttpStatusCode.Unauthorized
                };

                return Unauthorized(failedUnauthorized);
            }

            var server = await _context.Servers.FirstOrDefaultAsync(server => server.Id == serverId);

            if (server == null)
            {
                var failedNotFound = new RequestFailedResponse
                {
                    Success = false,
                    Message = "Server not found.",
                    StatusCode = HttpStatusCode.NotFound
                };

                return NotFound(failedNotFound);
            }

            if (!Permissions.HasServerPermissions(currentUser, server))
            {
                var failedUnauthorized = new RequestFailedResponse
                {
                    Success = false,
                    Message = "Only the owner of a server or a privileged user can delete a server.",
                    StatusCode = HttpStatusCode.Unauthorized
                };

                return Unauthorized(failedUnauthorized);
            }

            await _context.UserFavoritesListItems
                .Where(item =>
                    item.ServerName == server.ServerName &&
                    item.ServerAddress == server.ServerAddress &&
                    item.ServerPort == server.ServerPort)
                .ExecuteDeleteAsync();

            _context.Servers.Remove(server);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var badRequest = new RequestFailedResponse
                {
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"Something happened that no one was prepared for: {ex.Message}",
                };

                return BadRequest(badRequest);
            }

            var serverDeletedResponse = new BasicSuccessResponse
            {
                Success = true,
                Message = "Server deleted.",
                StatusCode = HttpStatusCode.NoContent
            };

            return Ok(serverDeletedResponse);
        }

        #endregion

        private async Task<bool> AddressInUse( string addr )
        {
            var exists = await _context.Servers.FirstOrDefaultAsync(u => string.Equals(u.ServerAddress.ToLower(), addr));

            if (exists != null)
                return true;

            return false;
        }
    }
}
