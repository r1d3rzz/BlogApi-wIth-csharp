using BlogApi.Models;
using log4net;
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

        public UsersController(ILog logger, BlogApiContext context)
        {
            this.logger = logger;
            this.context = context;
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

                var newUser = new User()
                {
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.Phone,
                    Password = user.Password,
                    Image = hasFile == true ? filename : null,
                };

                await context.Users.AddAsync(newUser);
                await context.SaveChangesAsync();

                return Ok(newUser);
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
                var user = await context.Users.FindAsync(id);
                bool hasEmail = await EmailExists(requestUser.Email);
                
                if (user == null)
                {
                    return NotFound();
                }

                if (!hasEmail)
                    return Ok("Email is not Valid");

                if (user.Email != requestUser.Email)
                {
                    logger.Warn("User Edit: Email is not Valid!");
                    return Ok("Email is Already Exists");
                }

                bool hasNewFile = requestUser.File != null ? true : false;
                string filename = "";

                if (hasNewFile)
                    filename = await WriteFile(requestUser.File);

                var Password = requestUser.Password != null ? requestUser.Password : user.Password;

                user.Name = requestUser.Name;
                user.Email = requestUser.Email;
                user.Phone = requestUser.Phone;
                user.Password = Password;
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
