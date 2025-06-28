using eShop.Api.DAL;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace eShop.Api.IntegrationTest.Common
{
    public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        /// <summary>
        /// docker run -d --name eshop_test -e POSTGRES_DB=eshop_test -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=password -p 5432:5432 postgres:latest
        /// </summary>
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithDatabase("eshop_test")
            .WithUsername("postgres")
            .WithPassword("password")
            .Build();

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();

            var factory = this.Services.GetRequiredService<IDbContextFactory<BlogContext>>();
            using var dbContext = await factory.CreateDbContextAsync();
            await dbContext.Database.MigrateAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                var descripor = services.SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<BlogContext>));

                if (descripor is not null)
                {
                    services.Remove(descripor);
                }
                services.AddDbContextFactory<BlogContext>(options =>
                {
                    options.UseNpgsql(_dbContainer.GetConnectionString())
                           .UseSnakeCaseNamingConvention();
                });
            });
            base.ConfigureWebHost(builder);
        }

        public async Task ResetDatabaseAsync()
        {
            var connectionString = this._dbContainer.GetConnectionString();
            var respawner = await Respawner.CreateAsync(connectionString, new RespawnerOptions
            {
                SchemasToInclude =
                [
                    "public"
                ],
                DbAdapter = DbAdapter.Postgres
            });

            await respawner.ResetAsync(connectionString);
        }

        public new async Task DisposeAsync()
        {
            var factory = this.Services.GetRequiredService<IDbContextFactory<BlogContext>>();
            using var dbContext = await factory.CreateDbContextAsync();
            using var conn = dbContext.Database.GetDbConnection();
            await conn.OpenAsync();
            var respawner = await Respawner.CreateAsync(
                conn,
                new RespawnerOptions
                {
                    SchemasToInclude = ["public", "postgres"],
                    DbAdapter = DbAdapter.Postgres
                }
                );
            await respawner.ResetAsync(conn);
            await conn.CloseAsync();
            await _dbContainer.StopAsync();
            await _dbContainer.DisposeAsync();
        }
    }
}
