using CounterStrike.REST.Models;
using CounterStrike.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CounterStrike.REST.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamsController : ControllerBase
    {
        private readonly ICrudServiceAsync<TeamModel> _service;

        public TeamsController(ICrudServiceAsync<TeamModel> service)
        {
            _service = service;
        }

        // Anyone can view teams
        [HttpGet]
        [AllowAnonymous]
        public async Task<IEnumerable<TeamModel>> GetAll() => await _service.ReadAllAsync();

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(Guid id)
        {
            var t = await _service.ReadAsync(id);
            if (t == null) return NotFound();
            return Ok(t);
        }

        // Create team → Commander or Admin
        [HttpPost]
        [Authorize(Roles = "Commander,Admin")]
        public async Task<IActionResult> Create([FromBody] TeamDto dto)
        {
            var team = new TeamModel
            {
                Name = dto.Name,
                Faction = dto.Faction
            };

            var created = await _service.CreateAsync(team);
            if (!created) return BadRequest();

            await _service.SaveAsync();
            return StatusCode(201);
        }

        // Update team → Commander or Admin
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Commander,Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TeamDto dto)
        {
            var exist = await _service.ReadAsync(id);
            if (exist == null) return NotFound();

            exist.Name = dto.Name;
            exist.Faction = dto.Faction;

            var ok = await _service.UpdateAsync(exist);
            if (!ok) return BadRequest();

            await _service.SaveAsync();
            return Ok();
        }

        // Delete team → only Admin
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var exist = await _service.ReadAsync(id);
            if (exist == null) return NotFound();

            var ok = await _service.RemoveAsync(exist);
            if (!ok) return BadRequest();

            await _service.SaveAsync();
            return Ok();
        }
    }
}
