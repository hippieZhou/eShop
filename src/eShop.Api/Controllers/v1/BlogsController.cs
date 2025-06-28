using Asp.Versioning;
using eShop.Api.DAL;
using eShop.Api.DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eShop.Api.Controllers.v1
{
    [ApiVersion(1)]
    [ApiController]
    [Route("api/v{v:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class BlogsController : ControllerBase
    {
        private readonly IDbContextFactory<BlogContext> _factory;
        private readonly ILogger<BlogsController> _logger;

        public BlogsController(IDbContextFactory<BlogContext> factory,ILogger<BlogsController> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        [MapToApiVersion(1)]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IEnumerable<Blog>> Get()
        {
            using var context = await _factory.CreateDbContextAsync();
            return await context.Blogs.ToListAsync();
        }

        [MapToApiVersion(1)]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            using var context = await _factory.CreateDbContextAsync();
            var item = await context.Blogs.FindAsync(id);

            if (item is null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [MapToApiVersion(1)]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(Blog item)
        {
            using var context = await _factory.CreateDbContextAsync();
            var entity = await context.Blogs.FindAsync(item.Id);
            if (entity is not null)
            {
                _logger.LogWarning("Blog with ID {Id} already exists.", item.Id);
                return BadRequest($"Blog with ID {item.Id} already exists.");
            }

            context.Blogs.Add(item);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }

        [MapToApiVersion(1)]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, Blog item)
        {
            using var context = await _factory.CreateDbContextAsync();
            var entity = await context.Blogs.FindAsync(id);
            if (entity is null)
            {
                _logger.LogWarning($"Blog with ID {id} doesn't exists.");
                return NotFound($"Blog with ID {id} doesn't exists.");
            }
            entity.Name = item.Name;
            entity.Url = item.Url;
            await context.SaveChangesAsync();
            return Ok(entity);
        }

        [MapToApiVersion(1)]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(int id)
        {
            using var context = await _factory.CreateDbContextAsync();
            var item = await context.Blogs.FindAsync(id);

            if (item is null)
            {
                _logger.LogWarning($"Blog with ID {id} doesn't exists.");
                return NotFound($"Blog with ID {id} doesn't exists.");
            }

            context.Blogs.Remove(item);
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
