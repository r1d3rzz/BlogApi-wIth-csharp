using BlogApi.Helpers;
using BlogApi.Models;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ILog logger;
        private readonly BlogApiContext context;
        private readonly IConfiguration configuration;

        public UsersController(ILog logger, BlogApiContext context, IConfiguration configuration)
        {
            this.logger = logger;
            this.context = context;
            this.configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> Index()
        {
            try
            {
                return await context.Users.ToListAsync();
            }
            catch (Exception ex)
            {
                logger.Error("User Index Error: " + ex);
                throw;
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> Show(int id)
        {
            try
            {
                var user = await context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                logger.Error("User Show: " + ex);
                throw;
            }
        }

        [HttpPost]
        public async Task<ActionResult<User>> Store([FromForm] User user)
        {
            try
            {
                bool hasFile = user.File != null ? true : false;
                bool hasEmail = await EmailExists(user.Email);
                string filename = "";

                if (hasFile)
                    filename = await WriteFile(user.File);

                if (hasEmail)
                {
                    logger.Warn("Your Email is Already Exists");
                    return Ok("Your Email is Already Exists");
                }


                string salt = PasswordManager.GenerateSalt();
                string hashedPassword = PasswordManager.HashPassword(user.Password, salt);

                var newUser = new User()
                {
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.Phone,
                    Password = hashedPassword + salt,
                    Image = hasFile == true ? filename : null,
                };

                await context.Users.AddAsync(newUser);
                await context.SaveChangesAsync();

                return Ok(TokenManager.CreateToken(configuration));
            }
            catch (Exception ex)
            {
                logger.Error("User Create: " + ex);
                throw;
            }
        }

        [HttpPut("Edit/{id}")]
        public async Task<ActionResult<User>> Edit(int id, [FromForm] User requestUser)
        {
            try
            {
                string newHashedPassword = "";
                var user = await context.Users.FindAsync(id);
                bool hasEmail = await EmailExists(requestUser.Email);

                if (user == null)
                {
                    return NotFound();
                }

                if (!hasEmail)
                    return Ok("Email is Already Exist!");

                if (user.Email != requestUser.Email)
                {
                    return Ok("Email is not valid");
                }

                bool hasNewFile = requestUser.File != null ? true : false;
                string filename = "";

                if (hasNewFile)
                {
                    if (user.Image != null)
                    {
                        System.IO.File.Delete(Path.Combine("wwwroot\\UsersImageFiles", user.Image));
                    }
                    filename = await WriteFile(requestUser.File);
                }

                if (requestUser.Password != null)
                {
                    if (!PasswordManager.IsTruePassword(requestUser.Password, user.Password))
                    {
                        return Ok("Old Password is not Correct!");
                    }
                    var newSalt = PasswordManager.GenerateSalt();
                    newHashedPassword = PasswordManager.HashPassword(requestUser.Password, newSalt) + newSalt;
                }

                user.Name = requestUser.Name;
                user.Email = requestUser.Email;
                user.Phone = requestUser.Phone;
                user.Password = requestUser.Password != null ? newHashedPassword : user.Password;
                user.Image = hasNewFile == true ? filename : user.Image;

                await context.SaveChangesAsync();
                return Ok(user);
            }
            catch (Exception ex)
            {
                logger.Error("User Update: " + ex);
                throw;
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult<User>> Delete(int id)
        {
            try
            {
                var user = await context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
                context.Remove(user);
                await context.SaveChangesAsync();
                return Ok(user);
            }
            catch (Exception ex)
            {
                logger.Error("User Delete: " + ex);
                throw;
            }
        }

        private async Task<string> WriteFile(IFormFile file)
        {
            string filename = "";
            try
            {
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                filename = DateTime.Now.Ticks.ToString() + extension;

                var filepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\UsersImageFiles");

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }

                var exactpath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\UsersImageFiles", filename);
                using (var stream = new FileStream(exactpath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                logger.Error("User Image File Writeline: " + ex);
            }
            return filename;
        }

        private async Task<bool> EmailExists(string UserMail)
        {
            var usersEmail = await context.Users.Select(x => x.Email).ToListAsync();

            if (usersEmail.Contains(UserMail))
            {
                return true;
            }

            return false;
        }
    }
}
