using AuthAPI.Context;
using AuthAPI.Helpers;
using AuthAPI.Interfaces;
using AuthAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace AuthAPI.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        public AuthController(AppDbContext dbContext, IUserService userService, IJwtService jwtService)
        {
            _userService = userService;
            _dbContext = dbContext;
            _jwtService = jwtService;

        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] User userModel)
        {
            var authResult = await _userService.AuthenticateUser(userModel);


            if (authResult.Success)
            {
                return Ok(new { Message = authResult.Message });
            }
            else
            {
                return BadRequest(new { Token = "", Message = authResult.Message });
            }
        }
        
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User userModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var registrationResult = await _userService.RegisterUser(userModel);

            if (!registrationResult.Success)
            {
                return BadRequest(new {Message = registrationResult.Message});
            }

            return Ok(new{Message = "User Registered"});
        }
        [Authorize]
        [HttpGet("api/User")]
        public async Task<ActionResult<User>> GetAllUsers()
        {
            return Ok(await _dbContext.Users.ToListAsync());
        }
        
     
    }
}
