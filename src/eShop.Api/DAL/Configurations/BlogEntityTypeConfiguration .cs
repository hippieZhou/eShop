using eShop.Api.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eShop.Api.DAL.Configurations
{
    public class BlogEntityTypeConfiguration : IEntityTypeConfiguration<Blog>
    {
        public void Configure(EntityTypeBuilder<Blog> builder)
        {
            builder.ToTable("Blogs").HasKey(b => b.Id);
            builder.Property(b => b.Id).UseSerialColumn();
            builder.Property(b => b.Name).IsRequired();
            builder.Property(b => b.Url).IsRequired();
        }
    }
}
