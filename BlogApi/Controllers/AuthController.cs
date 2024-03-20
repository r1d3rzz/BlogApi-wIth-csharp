using BlogApi.Helpers;
using BlogApi.Models;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlogApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILog logger;
        private readonly BlogApiContext context;
        private readonly IConfiguration configuration;

        public AuthController(ILog logger, BlogApiContext context, IConfiguration configuration)
        {
            this.logger = logger;
            this.context = context;
            this.configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(User request)
        {
            var user = await context.Users.Where(x => x.Email == request.Email).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("Your credentials is wrong");
            }

            if (PasswordManager.IsTruePassword(request.Password, user.Password))
            {
                return Ok(TokenManager.CreateToken(configuration));
            }

            return BadRequest("Password is Invalid");
        }
    }
}
