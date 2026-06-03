using BlazorApp.Infrastructure.Data;
using BlazorApp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BlazorApp.UnitTests;

public class ArchiveFileServiceTests
{
    private readonly ArchiveFileService _service;

    public ArchiveFileServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Archive:DefaultStoragePath"] = Path.GetTempPath(),
                ["Archive:MaxFileSizeMb"] = "50"
            })
            .Build();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("ArchiveTest")
            .Options;
        var db = new AppDbContext(options);

        _service = new ArchiveFileService(config, db);
    }

    [Theory]
    [InlineData("document.pdf", true)]
    [InlineData("photo.jpg", true)]
    [InlineData("spreadsheet.xlsx", true)]
    [InlineData("script.exe", false)]
    [InlineData("archive.zip", false)]
    public void IsAllowedExtension_ReturnsExpected(string fileName, bool expected)
    {
        Assert.Equal(expected, _service.IsAllowedExtension(fileName));
    }

    [Theory]
    [InlineData(1024 * 1024, true)]
    [InlineData(50L * 1024 * 1024, true)]
    [InlineData(51L * 1024 * 1024, false)]
    public void IsWithinSizeLimit_ReturnsExpected(long bytes, bool expected)
    {
        Assert.Equal(expected, _service.IsWithinSizeLimit(bytes));
    }
}
