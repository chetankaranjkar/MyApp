using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyAPI.Data;
using MyAPI.DTO;
using MyAPI.Models;

namespace MyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IAuthRepository _repro;
        private readonly IConfiguration _config;

        public AuthController(IAuthRepository repro, IConfiguration config)
        {
            this._repro = repro;
            this._config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]UserForRegisterDTO userForRegisterDTO) 
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            userForRegisterDTO.Username = userForRegisterDTO.Username.ToLower();
            if (await _repro.UserExists(userForRegisterDTO.Username))
                return BadRequest("User already exists");
            var userToCreate = new User
            {
                Username = userForRegisterDTO.Username
            };

            var createsUser = await _repro.Register(userToCreate, userForRegisterDTO.Password);

            return StatusCode(201);
        }
        [HttpPost("login")]
        public async Task<ActionResult> Login(UserForLoginDto userForLoginDTO)
        {
            var userFromRepo = await _repro.Login(userForLoginDTO.Username.ToLower(), userForLoginDTO.Password);

            if (userFromRepo == null)
                return Unauthorized();

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier,userFromRepo.ID.ToString()),
            new Claim(ClaimTypes.Name, userFromRepo.Username)
            };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new { token = tokenHandler.WriteToken(token) 
            });
             
        }
    }

    
}