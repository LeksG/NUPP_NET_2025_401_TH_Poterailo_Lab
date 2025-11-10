using CounterStrike.REST.Models;
using CounterStrike.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;

namespace CounterStrike.REST.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeaponsController : ControllerBase
    {
        private readonly ICrudServiceAsync<WeaponModel> _service;

        public WeaponsController(ICrudServiceAsync<WeaponModel> service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IEnumerable<WeaponModel>> GetAll() => await _service.ReadAllAsync();

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var w = await _service.ReadAsync(id);
            if (w == null) return NotFound();
            return Ok(w);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WeaponDto dto)
        {
            var weapon = new WeaponModel
            {
                Name = dto.Name,
                Damage = dto.Damage,
                Price = dto.Price,
                Type = dto.Type
            };

            var created = await _service.CreateAsync(weapon);
            if (!created) return BadRequest();

            await _service.SaveAsync();
            return StatusCode(201);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] WeaponDto dto)
        {
            var exist = await _service.ReadAsync(id);
            if (exist == null) return NotFound();

            exist.Name = dto.Name;
            exist.Damage = dto.Damage;
            exist.Price = dto.Price;
            exist.Type = dto.Type;

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
