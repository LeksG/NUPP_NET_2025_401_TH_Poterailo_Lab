using CounterStrike.REST.Models;
using CounterStrike.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;

namespace CounterStrike.REST.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly ICrudServiceAsync<PlayerModel> _service;

        public PlayersController(ICrudServiceAsync<PlayerModel> service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IEnumerable<PlayerModel>> GetAll() => await _service.ReadAllAsync();

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var p = await _service.ReadAsync(id);
            if (p == null) return NotFound();
            return Ok(p);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PlayerDto dto)
        {
            var player = new PlayerModel
            {
                Name = dto.Name,
                Health = dto.Health,
                Score = dto.Score,
                WeaponId = dto.WeaponId,
                TeamId = dto.TeamId
            };

            var created = await _service.CreateAsync(player);

            if (!created) return BadRequest();
            await _service.SaveAsync();
            return StatusCode(201);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PlayerDto dto)
        {
            var exist = await _service.ReadAsync(id);
            if (exist == null) return NotFound();

            exist.Name = dto.Name;
            exist.Health = dto.Health;
            exist.Score = dto.Score;
            exist.WeaponId = dto.WeaponId;
            exist.TeamId = dto.TeamId;

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
