
using Microsoft.AspNetCore.Mvc;
using DiscoverUO.Api.Models;
using DiscoverUO.Api.Data.Repositories.Contracts;

namespace DiscoverUO.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscoverUOServersController : ControllerBase
    {
        private readonly IServerDataRepository _dataRepository;
        
        public DiscoverUOServersController(IServerDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }

        [HttpGet("GetServers")]
        public async Task<ActionResult<IEnumerable<Server>>> GetServers()
        {
            try
            {
                var serverList = await _dataRepository.GetServers();

                if (serverList == null)
                    return NotFound();
                else
                    return serverList;
            }
            catch( Exception ex )
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetServer/{id}")]
        public async Task<ActionResult<Server>> GetServer( int id )
        {
            try
            {
                var server = await _dataRepository.GetServer(id); ;

                if (server == null)
                    return NotFound($"The server you are looking for, with Id = {id}, was not found.");
                else
                    return server;
            }
            catch( Exception ex )
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("PutServer/{id}")]
        public async Task<IActionResult> PutServer( int id, Server server )
        {
            try
            {
                if (id != server.Id)
                    return BadRequest($"Id[{id}] does not match the Id[{server.Id}] of the server you are updating. Id's much match.");

                if (!await _dataRepository.ServerExists(id))
                    return NotFound("That server doesn't exist.");

                var success = await _dataRepository.PutServer(server);

                if (!success)
                    return BadRequest("HttpPut failed.");

                return Ok(server);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("HttpPost")]
        public async Task<ActionResult<Server>> PostServer( Server server )
        {
            try
            {
                var success = await _dataRepository.PostServer(server);

                if ( success )
                    return CreatedAtAction("GetServer", new { id = server.Id }, server);
                else
                    return BadRequest();
            }
            catch ( Exception ex )
            {
                return BadRequest(ex);
            }
        }

        [HttpDelete("HttpDelete/{id}")]
        public async Task<IActionResult> DeleteServer( int id )
        {
            if( !await _dataRepository.ServerExists( id ) )
            {
                return NotFound();
            }

            bool success = await _dataRepository.DeleteServer(id);

            if (!success)
                return BadRequest();

            return NoContent();
        }
    }
}
