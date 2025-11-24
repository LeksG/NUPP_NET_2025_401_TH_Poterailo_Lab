using CounterStrike.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CounterStrike.REST.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email, FullName = dto.FullName };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            // default role
            await _userManager.AddToRoleAsync(user, "Player");

            return Ok(new { message = "Registered" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return Unauthorized();

            if (!await _userManager.CheckPasswordAsync(user, dto.Password)) return Unauthorized();

            var token = await GenerateJwtToken(user);
            return Ok(new { token });
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var jwt = _config.GetSection("JwtSettings");
            var secret = jwt.GetValue<string>("Secret") ?? "VeryLongSecretKey_ChangeThisToSomethingSecure_AtLeast32Chars";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
            };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var token = new JwtSecurityToken(
                issuer: jwt.GetValue<string>("Issuer") ?? "CounterStrikeApi",
                audience: jwt.GetValue<string>("Audience") ?? "CounterStrikeClients",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwt.GetValue<int>("ExpiryMinutes", 60)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public record RegisterDto(string Email, string Password, string FullName);
    public record LoginDto(string Email, string Password);
}
