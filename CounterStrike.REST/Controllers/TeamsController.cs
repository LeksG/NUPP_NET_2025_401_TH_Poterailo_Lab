using CounterStrike.REST.Models;
using CounterStrike.Infrastructure.Models;
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

        [HttpGet]
        public async Task<IEnumerable<TeamModel>> GetAll() => await _service.ReadAllAsync();

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var t = await _service.ReadAsync(id);
            if (t == null) return NotFound();
            return Ok(t);
        }

        [HttpPost]
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

        [HttpPut("{id:guid}")]
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

        [HttpDelete("{id:guid}")]
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
