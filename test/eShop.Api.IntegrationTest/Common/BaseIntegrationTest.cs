using eShop.Api.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace eShop.Api.IntegrationTest.Common
{
    public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>
    {
        protected readonly HttpClient Client;
        protected readonly BlogContext DbContext;

        protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
        {
            Client = factory.CreateDefaultClient();
            DbContext = factory.Services.GetRequiredService<IDbContextFactory<BlogContext>>().CreateDbContext();
        }
    }
}
