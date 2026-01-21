using FieldPro.Domain.Entities;
using FieldPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ===========================
// Database: PostgreSQL (Render)
// ===========================
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Normalizza sia postgres:// che postgresql://
    if (databaseUrl.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        databaseUrl = "postgres://" + databaseUrl.Substring("postgresql://".Length);
    }

    if (databaseUrl.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
    {
        var uri = new Uri(databaseUrl);

        var userInfo = uri.UserInfo.Split(':', 2);
        var user = userInfo[0];
        var password = userInfo.Length > 1 ? userInfo[1] : string.Empty;
        var host = uri.Host;
        var port = uri.Port;
        var db = uri.AbsolutePath.TrimStart('/');

        databaseUrl =
            $"Host={host};Port={port};Database={db};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
    }
}

// Fallback locale se DATABASE_URL non è impostata
var connectionString = string.IsNullOrEmpty(databaseUrl)
    ? builder.Configuration.GetConnectionString("DefaultConnection")
      ?? "Host=localhost;Port=5432;Database=fieldpro;Username=fieldpro;Password=fieldPro2026!"
    : databaseUrl;

builder.Services.AddDbContext<FieldProDbContext>(options =>
    options.UseNpgsql(connectionString));


// ===========================
// CORS (aperto per debug)
// ===========================
const string CorsPolicyName = "FrontendCors";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: CorsPolicyName, policy =>
    {
        policy
            .AllowAnyOrigin()   // per ora aperto: chiama da Netlify, localhost, ecc.
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ===========================
// App
// ===========================
var app = builder.Build();

app.UseCors(CorsPolicyName);

app.MapGet("/", () => "FieldPro API");

// GET technicians
app.MapGet("/technicians", async (FieldProDbContext db) =>
{
    var technicians = await db.Technicians
        .OrderBy(t => t.Name)
        .ToListAsync();

    return Results.Ok(technicians);
});

// GET jobs (paginato + filtri + includeArchived -> IsDeleted)
app.MapGet("/jobs", async (
    FieldProDbContext db,
    int page = 1,
    int pageSize = 20,
    string? status = null,
    string? search = null,
    bool includeArchived = false
) =>
{
    var query = db.Jobs
        .Include(j => j.Technician)
        .AsQueryable();

    if (!includeArchived)
    {
        query = query.Where(j => !j.IsDeleted);
    }

    if (!string.IsNullOrWhiteSpace(status))
    {
        query = query.Where(j => j.Status == status);
    }

    if (!string.IsNullOrWhiteSpace(search))
    {
        var s = search.ToLower();
        query = query.Where(j =>
            j.Code.ToLower().Contains(s) ||
            j.CustomerName.ToLower().Contains(s) ||
            (j.Project != null && j.Project.ToLower().Contains(s)));
    }

    query = query
        .OrderBy(j => j.ScheduledAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize);

    var jobs = await query
        .Select(j => new
        {
            j.Id,
            j.Code,
            j.CustomerName,
            j.Address,
            j.ScheduledAt,
            j.CompletedAt,
            j.Status,
            j.Project,
            j.TechnicianId,
            TechnicianName = j.Technician != null ? j.Technician.Name : null,
            j.Notes,
            j.IsDeleted,
            j.DeletedAt
        })
        .ToListAsync();

    return Results.Ok(jobs);
});

// POST job
app.MapPost("/jobs", async (FieldProDbContext db, JobCreateRequest request) =>
{
    var job = new Job
    {
        Code = request.Code,
        CustomerName = request.CustomerName,
        Address = request.Address,
        ScheduledAt = request.ScheduledAt,
        Status = request.Status,
        Project = request.Project,
        TechnicianId = request.TechnicianId,
        IsDeleted = false,
        DeletedAt = null
    };

    db.Jobs.Add(job);
    await db.SaveChangesAsync();

    return Results.Created($"/jobs/{job.Id}", job);
});

// PUT status/notes
app.MapPut("/jobs/{id:int}", async (FieldProDbContext db, int id, JobUpdateStatusRequest request) =>
{
    var job = await db.Jobs.FindAsync(id);
    if (job == null)
    {
        return Results.NotFound();
    }

    job.Status = request.Status;
    job.Notes = request.Notes;

    if (request.Status == "Completed" && job.CompletedAt == null)
    {
        job.CompletedAt = DateTime.UtcNow;
    }

    await db.SaveChangesAsync();

    return Results.NoContent();
});

// DELETE job -> soft delete
app.MapDelete("/jobs/{id:int}", async (FieldProDbContext db, int id) =>
{
    var job = await db.Jobs.FindAsync(id);
    if (job == null)
    {
        return Results.NotFound();
    }

    job.IsDeleted = true;
    job.DeletedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();

// ===========================
// DTO
// ===========================
public record JobCreateRequest(
    string Code,
    string CustomerName,
    string Address,
    DateTime ScheduledAt,
    string Status,
    string? Project,
    int? TechnicianId
);

public record JobUpdateStatusRequest(
    string Status,
    string? Notes
);
