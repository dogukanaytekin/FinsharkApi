using api.Dtos.Account;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<AppUser> _signInManager;
        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userManager.Users.ToListAsync();
            return Ok(users);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if(!ModelState.IsValid)
                return BadRequest();
            
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == loginDto.UserName);

            if (user == null) return Unauthorized("Invalid username!");

            var result = _signInManager.CheckPasswordSignInAsync(user , loginDto.Password , false);

            if (result == null) return Unauthorized("Invalid username or password.");
            try{
            return Ok(
                new NewUserDto
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    Token = _tokenService.CreateToken(user)
                }
            );
            }
            catch(Exception e) {
                return StatusCode(410, "_key'de bir hata var");
            }


        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto){
            try
            {
                if(!ModelState.IsValid)
                return BadRequest(ModelState);

                var appUser = new AppUser
                {
                    UserName = registerDto.Username,
                    Email = registerDto.Email
                };


                var createdUser = await _userManager.CreateAsync(appUser, registerDto.Password);

                if(createdUser.Succeeded)
                {
                    var roleresult = await _userManager.AddToRoleAsync(appUser, "User");

                    if (roleresult.Succeeded)
                    {
                        return Ok(new NewUserDto{
                            UserName = appUser.UserName,
                            Email = appUser.Email,
                            Token = _tokenService.CreateToken(appUser)
                        });
                    }
                    else 
                    {
                        return StatusCode(500 , roleresult.Errors);
                    }
                }
                else 
                {
                    return StatusCode(500 , createdUser.Errors);
                }

            }
            catch(Exception e)
            {
                return StatusCode(500, e);
            }
            
        }
    }
}