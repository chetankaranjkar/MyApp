using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public AuthController(IAuthRepository repro)
        {
            this._repro = repro;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDTO userForRegisterDTO) 
        {
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

    }
}