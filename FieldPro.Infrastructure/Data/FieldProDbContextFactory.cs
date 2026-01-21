using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FieldPro.Infrastructure.Data;

public class FieldProDbContextFactory : IDesignTimeDbContextFactory<FieldProDbContext>
{
    public FieldProDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        var apiProjectPath = Path.Combine(basePath, "FieldPro.Api");
        if (!Directory.Exists(apiProjectPath))
        {
            apiProjectPath = Path.Combine(basePath, "..", "FieldPro.Api");
        }

        Microsoft.Extensions.Configuration.IConfigurationBuilder builder =
            new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .SetBasePath(apiProjectPath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables();

        IConfiguration configuration = builder.Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<FieldProDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new FieldProDbContext(optionsBuilder.Options);
    }
}
