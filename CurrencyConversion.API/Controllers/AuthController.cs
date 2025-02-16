using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CurrencyConversion.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration config;

        public AuthController(IConfiguration config)
        {
            this.config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            //request.Username = "admin";
            //request.Password = "password";
            if (IsValidUser(request))
            {
                var token = GenerateJwtToken(request.Username);
                return Ok(new { Token = token });
            }
            return Unauthorized();
        }

        private bool IsValidUser(LoginRequest request)
        {
            return request.Username == "admin" && request.Password == "password";
        }

        private string GenerateJwtToken(string username)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("role", "Admin"), // Add admin roles for RBAC
            new Claim("role", "Guest")// Add Guest roles for RBAC 
        };

            var token = new JwtSecurityToken(
                issuer: this.config["Jwt:Issuer"],
                audience: null,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(48),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}
