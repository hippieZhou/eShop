using System.Net.Http.Json;
using eShop.Api.DAL.Entities;
using eShop.Api.IntegrationTest.Common;
using Microsoft.EntityFrameworkCore;

namespace eShop.Api.IntegrationTest
{
    public class BlogsControllerTests : BaseIntegrationTest
    {
        public BlogsControllerTests(IntegrationTestWebAppFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task Create_ShouldAdd_NewBlogToDatabase()
        {
            var newBlog = new Blog
            {
                Name = "Test Blog",
                Url = "https://example.com/blogs/1",
            };

            var response = await Client.PostAsJsonAsync("/api/v1/blogs", newBlog);
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);
            var sult = await response.Content.ReadFromJsonAsync<Blog>();
            Assert.NotNull(sult);

            var blog = await DbContext.Blogs.AsNoTracking().FirstOrDefaultAsync(b => b.Name == newBlog.Name && b.Url == newBlog.Url);
            Assert.NotNull(blog);
            Assert.Equal(sult.Id, blog?.Id);
            Assert.Equal(newBlog.Name, blog?.Name);
            Assert.Equal(newBlog.Url, blog?.Url);
        }
    }
}
