using eShop.Api.DAL.Configurations;
using Microsoft.EntityFrameworkCore;

namespace eShop.Api.DAL.Entities
{
    [EntityTypeConfiguration(typeof(BlogEntityTypeConfiguration))]
    public class Blog
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Url { get; set; }
    }
}
