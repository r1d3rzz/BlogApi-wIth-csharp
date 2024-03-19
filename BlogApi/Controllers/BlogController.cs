using BlogApi.Models;
using BlogApi.Models.Create;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly ILog logger;
        private readonly BlogApiContext context;

        public BlogController(ILog logger, BlogApiContext context)
        {
            this.logger = logger;
            this.context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Blog>>> Index()
        {
            try
            {
                return await context.Blogs.Include(x => x.Category).ToListAsync();
            }
            catch (Exception ex)
            {
                logger.Error("This is Error Msg: " + ex);
                throw;
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Blog>> Show(int id)
        {
            try
            {
                var blog = await context.Blogs.FindAsync(id);
                if (blog == null)
                {
                    return NotFound();
                }
                var data = await context.Blogs.Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == id);
                return data;
            }
            catch (Exception ex)
            {
                logger.Error("Blog Show: " + ex);
                throw;
            }
        }

        [HttpPost]
        public async Task<ActionResult<Blog>> Store(BlogCreate blog)
        {
            try
            {
                var categoryIds = await context.Categories.Select(c => c.Id).ToListAsync();

                if (!categoryIds.Contains(blog.CategoryId))
                {
                    logger.Error("Blog Store: Category Id is not valid!");
                    return BadRequest();
                }

                var data = new Blog()
                {
                    Title = blog.Title,
                    Body = blog.Body,
                    CategoryId = blog.CategoryId,
                };

                await context.AddAsync(data);
                await context.SaveChangesAsync();
                return Ok("Created Successful");
            }
            catch (Exception ex)
            {
                logger.Error("Blog Store: " + ex.Message);
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Blog blog)
        {
            try
            {
                var data = await context.Blogs.FindAsync(id);
                if (data != null)
                {
                    data.Id = id;
                    data.Title = blog.Title;
                    data.Body = blog.Body;
                    data.CategoryId = blog.CategoryId;

                    await context.SaveChangesAsync();
                    return CreatedAtAction("Show", new { id = data.Id }, data);
                }
                logger.Warn("Blog Update: Blog not found!");
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.Error("Blog Update: " + ex);
                throw;
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Blog>> Destory(int id)
        {
            try
            {
                var blog = await context.Blogs.FindAsync(id);

                if (blog != null)
                {
                    context.Blogs.Remove(blog);
                    await context.SaveChangesAsync();
                    return NoContent();
                }

                logger.Warn("Blog Delete: Blog Not Found");
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.Error("Blog Destroy: " + ex);
                throw;
            }
        }
    }
}
