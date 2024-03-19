using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogApi.Models;
using log4net.Config;
using log4net.Core;
using log4net;
using System.Reflection;

namespace BlogApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly BlogApiContext _context;
        private readonly ILog _logger;

        public CategoriesController(BlogApiContext context, ILog logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            try
            {
                return await _context.Categories.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }

        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);

                if (category == null)
                {
                    return NotFound();
                }

                return category;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }

        }

        // PUT: api/Categories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, Category category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    _logger.Error(ex.Message);
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Categories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {
            try
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetCategory", new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return NotFound();
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw;
            }
        }
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
