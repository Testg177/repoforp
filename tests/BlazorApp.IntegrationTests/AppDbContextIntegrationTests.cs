using BlazorApp.Domain.Entities;
using BlazorApp.Domain.Enums;
using BlazorApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace BlazorApp.IntegrationTests;

public class AppDbContextIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithImage("postgres:16-alpine")
        .WithDatabase("blazorapp_test")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .Build();

    private AppDbContext _db = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString(), npgsql =>
                npgsql.MigrationsAssembly("BlazorApp.Infrastructure"))
            .Options;

        _db = new AppDbContext(options);
        await _db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task CanSaveAndRetrieveArchiveCategory()
    {
        var category = new ArchiveCategory
        {
            Name = "Test Category",
            Code = "TEST",
            IsActive = true
        };

        _db.ArchiveCategories.Add(category);
        await _db.SaveChangesAsync();

        var retrieved = await _db.ArchiveCategories
            .FirstOrDefaultAsync(c => c.Code == "TEST");

        Assert.NotNull(retrieved);
        Assert.Equal("Test Category", retrieved.Name);
    }

    [Fact]
    public async Task CanSaveFormDefinition()
    {
        var form = new FormDefinition
        {
            Code = "TEST_FORM",
            Title = "Test Form",
            IsActive = true,
            RequiresApproval = true,
            AssignedRoles = ["Employee"],
            AssignedDepartments = ["IT"]
        };

        _db.FormDefinitions.Add(form);
        await _db.SaveChangesAsync();

        var retrieved = await _db.FormDefinitions
            .FirstOrDefaultAsync(f => f.Code == "TEST_FORM");

        Assert.NotNull(retrieved);
        Assert.Contains("Employee", retrieved.AssignedRoles);
    }
}
