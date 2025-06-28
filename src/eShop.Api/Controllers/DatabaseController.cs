using eShop.Api.DAL;
using eShop.Api.DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eShop.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DatabaseController : ControllerBase
    {

        private readonly IDbContextFactory<BlogContext> _DbContextFactory;

        public DatabaseController(IDbContextFactory<BlogContext> dbContextFactory)
        {
            _DbContextFactory = dbContextFactory;
        }

        /// <summary>对数据库应用EF迁移。如果数据库不存在，则创建数据库。</summary>
        [HttpPost("apply-migrations")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async ValueTask<IActionResult> ApplyMigrations()
        {
            await using var dbContext = await _DbContextFactory.CreateDbContextAsync();
            await dbContext.Database.MigrateAsync();
            return NoContent();
        }

        [HttpPost("apply-seeding")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async ValueTask<IActionResult> ApplySeeds()
        {
            await using var dbContext = await _DbContextFactory.CreateDbContextAsync();
            if (await dbContext.Blogs.AsNoTracking().AnyAsync())
            {
                return NoContent();
            }
            await dbContext.Blogs.AddRangeAsync(new List<Blog>
            {
                new Blog { Name = "Blog 1", Url = "https://example.com/blog1" },
                new Blog { Name = "Blog 2", Url = "https://example.com/blog2" },
                new Blog { Name = "Blog 3", Url = "https://example.com/blog3" }
            });
            await dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
