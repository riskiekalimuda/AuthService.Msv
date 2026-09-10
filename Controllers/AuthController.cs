using AuthService.Msv.DTOs;
using AuthService.Msv.Models;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Msv.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthMsvDbContext _dbContext;
        private readonly ILogger<AuthController> _logger;
        private readonly AuthService.Msv.Services.AuthService _authService;
        public AuthController(AuthMsvDbContext dbContext, ILogger<AuthController> logger, AuthService.Msv.Services.AuthService authService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]UserDto request)
        {
            try
            {
                var result = await _authService.RegisterUser(request);
                if (!result.IsSuccess){
                    return BadRequest(result.ErrorMessage);
                }
                return Ok(new { Data=result.Data, Message="Save register user has succesfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);  
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDto login)
        {
            var result = await _authService.Login(login); 
            return Ok(new { Token=result.Data});
        }
    }
}
