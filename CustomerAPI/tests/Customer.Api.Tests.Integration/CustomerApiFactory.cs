using Bogus;
using CustomerAPI.Data;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Respawn;
using System.Data.Common;

namespace Customer.Api.Tests.Integration;

public class CustomerApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlTestcontainer _dbContainer;

    private string _connectionString = default!;
    private DbConnection _dbConnection = default!;
    private Respawner _respawner = default!;

    public HttpClient HttpClient { get; private set; } = default!;

    public static Faker<CustomerAPI.Models.Customer> CustomerGenerator => new Faker<CustomerAPI.Models.Customer>()
            .RuleFor( c=> c.Id,f => Guid.NewGuid())
            .RuleFor(c => c.FirstName, f => f.Person.FirstName)
            .RuleFor(c => c.LastName, f=> f.Person.LastName);

    public CustomerApiFactory()
    {
        _dbContainer = new TestcontainersBuilder<MsSqlTestcontainer>()
            .WithDatabase(new MsSqlTestcontainerConfiguration
            {
                Database = "CustomerAPI",
                Password = "Password!!",
            })
            .Build();
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<CustomerDbContext>>();
            services.AddDbContext<CustomerDbContext>(options => { options.UseSqlServer(_connectionString); });

            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var context = scopedServices.GetRequiredService<CustomerDbContext>();
            context.Database.EnsureCreated();
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        _connectionString = $"{_dbContainer.ConnectionString}TrustServerCertificate=True;";
        _dbConnection = new SqlConnection(_connectionString);

        HttpClient = CreateClient();

        await InitializeRespawner();
    }

    public async Task ResetDatabase() => await _respawner.ResetAsync(_dbConnection);
     
    private async Task InitializeRespawner()
    {
        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = new[] { "dbo" }
        });
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }
}
