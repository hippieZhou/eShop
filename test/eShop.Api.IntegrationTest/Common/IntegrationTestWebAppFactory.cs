using DotNet.Testcontainers.Builders;
using eShop.Api.DAL;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Testcontainers.PostgreSql;

namespace eShop.Api.IntegrationTest.Common
{
    public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private int Port = Random.Shared.Next(10000, 60000);
        private readonly PostgreSqlContainer _dbContainer;

        /// <summary>
        /// docker run -d --name eshop_test -e POSTGRES_DB=eshop_test -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=password -p 5432:5432 postgres:latest
        /// </summary>
        public IntegrationTestWebAppFactory()
        {
            _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithDatabase("eshop_test")
            .WithUsername("postgres")
            .WithPassword("password")
            .WithPortBinding(Port, 5432)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            #region Option 1
            builder.UseSetting("ConnectionStrings:Database", _dbContainer.GetConnectionString());
            #endregion

            #region Option 2
            //builder.ConfigureTestServices(services =>
            //{
            //    services.RemoveAll(typeof(DbContextOptions<BlogContext>));
            //    services.AddSingleton(sp =>
            //    {
            //        var optionsBuilder = new DbContextOptionsBuilder<BlogContext>();
            //        optionsBuilder.UseNpgsql(_dbContainer.GetConnectionString())
            //                      .UseSnakeCaseNamingConvention();
            //        return optionsBuilder.Options;
            //    });
            //});
            #endregion

            base.ConfigureWebHost(builder);
        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();

            var factory = this.Services.GetRequiredService<IDbContextFactory<BlogContext>>();
            using var dbContext = await factory.CreateDbContextAsync();
            await dbContext.Database.MigrateAsync();
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
