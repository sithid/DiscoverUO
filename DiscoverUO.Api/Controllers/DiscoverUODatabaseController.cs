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

        [Authorize]
        [HttpPut("UpdateServer/{id}")]
        public async Task<IActionResult> UpdateServer(int id, ServerDto serverDto)
        {
            var server = await _context.Servers.FindAsync(id);

            if (server == null)
            {
                return NotFound();
            }

            server = _mapper.Map<Server>(serverDto);

            _context.Entry(server).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ServerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return BadRequest($"Something happened that no one was prepared for.\n\r{ex}");
                }
            }

            var updatedServer = await _context.Servers
                .FirstOrDefaultAsync(s => s.Id == id);

            var updatedServerDto = _mapper.Map<ServerDto>(updatedServer);

            return Ok(updatedServerDto);
        }

        [Authorize]
        [HttpPost("CreateServer")]
        public async Task<ActionResult<ServerDto>> CreateServer(ServerDto serverDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var server = _mapper.Map<Server>(serverDto);

            _context.Servers.Add(server);
            await _context.SaveChangesAsync();

            var newServerDto = _mapper.Map<ServerDto>(server);

            return CreatedAtAction("GetServer", new { id = server.Id }, serverDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServer(int id)
        {
            var server = await _context.Servers.FindAsync(id);
            if (server == null)
            {
                return NotFound();
            }

            _context.Servers.Remove(server);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ServerExists(int id)
        {
            return _context.Servers.Any(e => e.Id == id);
        }
    }
}
