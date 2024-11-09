using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiscoverUO.Api;
using DiscoverUO.Api.Models;

using Microsoft.AspNetCore.Authorization;
using DiscoverUO.Lib.DTOs.Servers;
using DiscoverUO.Lib.DTOs.Users;
using Microsoft.AspNetCore.Hosting.Server;
using AutoMapper;
using System.Security.Claims;

namespace DiscoverUO.Api.Controllers
{
    [Route("api/servers")]
    [ApiController]
    public class DiscoverUOServersController : ControllerBase
    {
        private readonly DiscoverUODatabaseContext _context;
        private readonly IMapper _mapper;

        public DiscoverUOServersController(DiscoverUODatabaseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet("All")]
        public async Task<ActionResult<IEnumerable<ServerDto>>> GetServers()
        {
            var servers = await _context.Servers.ToListAsync();
            var serverDtos = _mapper.Map<List<ServerDto>>(servers);

            return Ok(serverDtos);
        }

        [AllowAnonymous]
        [HttpGet("ById/{id}")]
        public async Task<ActionResult<ServerDto>> GetServer(int id)
        {
            var server = await _context.Servers.FindAsync(id);

            if (server == null)
            {
                return NotFound();
            }

            var serverDto = _mapper.Map<ServerDto>(server);

            return Ok(serverDto);
        }

        [AllowAnonymous]
        [HttpGet("OwnedServers")]
        public async Task<ActionResult<List<ServerDto>>> GetOwnedServers()
        {
            var servers = await _context.Servers.Where(s => HasModifyPermission(s.OwnerId)).ToListAsync();
            var serverDtos = _mapper.Map<List<ServerDto>>(servers);

            return Ok(serverDtos);
        }

        [Authorize]
        [HttpPost("CreateServer")]
        public async Task<ActionResult<ServerDto>> CreateServer(ServerUpdateDto serverDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var server = _mapper.Map<Server>(serverDto);

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in claims.");
            }

            server.OwnerId = int.Parse(userIdClaim.Value);

            _context.Servers.Add(server);
            await _context.SaveChangesAsync();

            var createdServer = await _context.Servers
                .FirstOrDefaultAsync(s => s.Id == server.Id);

            var createdServerDto = _mapper.Map<ServerDto>(createdServer);

            return CreatedAtAction("GetServer", new { id = createdServer.Id }, createdServerDto);
        }

        [Authorize]
        [HttpPut("UpdateServer/{serverId}")]
        public async Task<IActionResult> UpdateServer(int serverId, ServerUpdateDto serverDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var server = await _context.Servers.FindAsync(serverId);

            if (server == null)
            {
                return NotFound();
            }

            if (!HasModifyPermission(server.OwnerId))
            {
                return Unauthorized("Only the server owner can delete this server.");
            }

            server = _mapper.Map<Server>(serverDto);

            _context.Entry(server).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ServerExists(serverId))
                {
                    return NotFound();
                }
                else
                {
                    return BadRequest($"Something happened that no one was prepared for.\n\r{ex}");
                }
            }

            var updatedServer = await _context.Servers
                .FirstOrDefaultAsync(s => s.Id == serverId);

            var updatedServerDto = _mapper.Map<ServerDto>(updatedServer);

            return Ok(updatedServerDto);
        }

        [Authorize(Policy = "Privileged")]
        [HttpPut("admin/UpdateServerOwner/{serverId}")]
        public async Task<IActionResult> UpdateServerOwner( int serverId, int ownerId )
        {
            var server = await _context.Servers.FindAsync(serverId);

            if (server == null)
            {
                return NotFound("Server not found.");
            }

            var newOwner = await _context.Users.FindAsync(ownerId);

            if (newOwner == null)
            {
                return NotFound("New owner not found.");
            }

            server.OwnerId = ownerId;

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
                .FirstOrDefaultAsync(s => s.Id == serverId);

            var serverDto = _mapper.Map<ServerDto>(updatedServer);

            return Ok(serverDto);

        }

        [Authorize(Policy = "Privileged")]
        [HttpDelete("admin/DeleteServer/{id}")]
        public async Task<IActionResult> DeleteServer(int serverId)
        {
            var server = await _context.Servers.FindAsync(serverId);

            if (server == null)
            {
                return NotFound();
            }

            if(!HasModifyPermission( server.OwnerId ))
            {
                return Unauthorized("Only the server owner can delete this server.");
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

        private bool HasModifyPermission( int serverOwnerId )
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            var userRoleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

            if (userIdClaim == null)
            {
                return false;
            }

            int userId = int.Parse(userIdClaim.Value);
            string userRole = userRoleClaim.Value;

            return (userId == serverOwnerId || userRole == "Admin" || userRole == "Owner");
        }

        private bool ServerExists(int id)
        {
            return _context.Servers.Any(e => e.Id == id);
        }
    }
}
