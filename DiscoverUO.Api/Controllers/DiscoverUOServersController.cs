using AutoMapper;
using DiscoverUO.Api.Models;
using DiscoverUO.Lib.Shared.Servers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        [HttpGet("public")]
        public async Task<ActionResult<IEnumerable<ServerData>>> GetServers()
        {
            var servers = await _context.Servers.ToListAsync();
            return _mapper.Map<List<ServerData>>(servers);
        }

        [AllowAnonymous]
        [HttpGet("view/id/{id}")]
        public async Task<ActionResult<ServerData>> GetServerById(int id)
        {
            var server = await _context.Servers
                .FirstOrDefaultAsync(server => server.Id == id);

            if (server == null)
            {
                return NotFound();
            }

            var serverDto = _mapper.Map<ServerData>(server);

            return Ok(serverDto);
        }

        [AllowAnonymous]
        [HttpGet("view/name/{serverName}")]
        public async Task<ActionResult<int>> GetServerByName(string serverName)
        {
            var server = await _context.Servers
                .FirstOrDefaultAsync(s => string.Equals(s.ServerName, serverName));

            if (server == null)
            {
                return NotFound();
            }

            var serverDto = _mapper.Map<ServerData>(server);

            return Ok(serverDto);
        }

        [AllowAnonymous]
        [HttpGet("view/owner/{userName}")]
        public async Task<ActionResult<List<ServerData>>> FindServersByOwner(string userName)
        {
            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                return Unauthorized($"You must be logged in to do this.");
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => string.Equals(u.UserName, userName));

            if (existingUser == null)
            {
                return NotFound("The user does not exist.");
            }

            var ownedServers = await _context.Servers
                .Where(server => server.OwnerId == existingUser.Id)
                .ToListAsync();

            if (ownedServers == null)
            {
                return NotFound("That user doesnt own any servers.");
            }

            var serverDtos = _mapper.Map<List<ServerData>>(ownedServers);

            return Ok(serverDtos);
        }

        #endregion

        #region BasicUser+ Endpoints

        [Authorize]
        [HttpGet("view/owned")]
        public async Task<ActionResult<List<ServerData>>> GetOwnedServers()
        {
            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                return Unauthorized($"You must be logged in to do this.");
            }

            var ownedServers = await _context.Servers
                .Where(server => server.OwnerId == currentUser.Id)
                .ToListAsync();

            if (ownedServers == null)
            {
                return NotFound("You do not own any servers.");
            }

            var serverDtos = _mapper.Map<List<ServerData>>(ownedServers);

            return Ok(serverDtos);
        }

        [Authorize]
        [HttpPost("CreateServer")]
        public async Task<ActionResult<ServerData>> AddServer(ServerRegistrationData createServerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                return Unauthorized("You must be logged in to do this.");
            }

            var serverToAdd = _mapper.Map<Server>(createServerDto);
            serverToAdd.OwnerId = currentUser.Id;

            _context.Servers.Add(serverToAdd);

            await _context.SaveChangesAsync();

            var createdServer = await _context.Servers
                .FirstOrDefaultAsync(s => s.Id == serverToAdd.Id);

            var createdServerDto = _mapper.Map<ServerData>(createdServer);

            return CreatedAtAction("GetServerById", new { id = createdServer.Id }, createdServerDto);
        }

        [Authorize]
        [HttpPut("update/UpdateServer/{serverId}")]
        public async Task<ActionResult<ServerData>> UpdateServer(int serverId, ServerUpdateData serverUpdateDto)
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

            var serverToUpdate = await _context.Servers.FirstOrDefaultAsync(server => server.Id == serverId);

            if (serverToUpdate == null)
            {
                return NotFound("Server not found.");
            }

            if (!Permissions.HasServerPermissions(currentUser, serverToUpdate))
            {
                return Unauthorized("Only the owner of a server or a privileged user can update a servers information.");
            }

            serverToUpdate.ServerName = serverUpdateDto.ServerName;
            serverToUpdate.ServerAddress = serverUpdateDto.ServerAddress;
            serverToUpdate.ServerPort = serverUpdateDto.ServerPort;
            serverToUpdate.ServerEra = serverUpdateDto.ServerEra;
            serverToUpdate.PvPEnabled = serverUpdateDto.PvPEnabled;
            serverToUpdate.IsPublic = serverUpdateDto.IsPublic;

            _context.Entry(serverToUpdate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return BadRequest($"Something happened that no one was prepared for: {ex}");
            }

            var updatedServer = await _context.Servers
                .FirstOrDefaultAsync(s => s.Id == serverToUpdate.Id);

            var updatedServerDto = _mapper.Map<ServerData>(updatedServer);

            return Ok(updatedServerDto);
        }

        [Authorize]
        [HttpPut("ownership/transfer/{serverId}_{newOwnerId}")]
        public async Task<ActionResult<ServerData>> TransferOwnership(int serverId, int newOwnerId)
        {
            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                return Unauthorized($"You must be logged in to do this.");
            }

            var server = await _context.Servers.FirstOrDefaultAsync(s => s.Id == serverId);

            if (server == null)
            {
                return NotFound("Server not found.");
            }

            if (!Permissions.HasServerPermissions(currentUser, server))
            {
                return Unauthorized("Only the owner of a server or a privileged user can transfer server ownership.");
            }

            var newOwner = await _context.Users.FirstOrDefaultAsync(user => user.Id == newOwnerId);

            if (newOwner == null)
            {
                return NotFound("New owner not found.");
            }

            server.OwnerId = newOwner.Id;

            _context.Entry(server).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the server owner: {ex.Message}");
            }

            var updatedServer = await _context.Servers
                .FirstOrDefaultAsync(server => server.Id == serverId);

            var serverDto = _mapper.Map<ServerData>(updatedServer);

            return Ok(serverDto);

        }

        [Authorize]
        [HttpDelete("delete/{serverId}")]
        public async Task<ActionResult> DeleteServer(int serverId)
        {
            var currentUser = await Permissions.GetCurrentUser(this.User, _context);

            if (currentUser == null)
            {
                return Unauthorized($"You must be logged in to do this.");
            }

            var server = await _context.Servers.FirstOrDefaultAsync(server => server.Id == serverId);

            if (server == null)
            {
                return NotFound();
            }

            if (!Permissions.HasServerPermissions(currentUser, server))
            {
                return Unauthorized("Only the server owner or a privileged user can delete a server.");
            }

            await _context.UserFavoritesListItems
                .Where(item =>
                    item.ServerName == server.ServerName &&
                    item.ServerAddress == server.ServerAddress &&
                    item.ServerPort == server.ServerPort)
                .ExecuteDeleteAsync();

            _context.Servers.Remove(server);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        #endregion
    }
}
