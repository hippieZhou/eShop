using eShop.Api.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace eShop.Api.DAL
{
    /*
     * docker run -d --name eshop -e POSTGRES_DB=eshop -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=password -p 5432:5432 postgres:latest
     */
    /// <summary>
    /// dotnet ef migrations add InitialCreate
    /// dotnet ef database update
    /// </summary>
    public class BlogContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public BlogContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Blog>();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
    }
}
