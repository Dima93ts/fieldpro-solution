using FieldPro.Domain.Entities;
using FieldPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("=== FieldPro API BOOT v3 ===");

// ===========================
// Database
// ===========================

// Connection string per Render (produzione)
const string renderConnectionString =
    "Host=dpg-d5oauavgi27c73ej0960-a;" +
    "Port=5432;" +
    "Database=fieldpro_db;" +
    "Username=fieldpro_db_user;" +
    "Password=TW6ambCRfjBcqgj91DqAhrtN6xHZ9nqn;" +
    "SSL Mode=Require;" +
    "Trust Server Certificate=true;";

// Connection string locale (sviluppo)
const string localConnectionString =
    "Host=localhost;Port=5432;Database=fieldpro;Username=fieldpro;Password=fieldPro2026!";

// Scegli quale usare: in produzione (Render) useremo quella Render
var env = builder.Environment.EnvironmentName;
var connectionString = string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase)
    ? localConnectionString
    : renderConnectionString;

Console.WriteLine($"[DEBUG] Using connection string: {connectionString}");

builder.Services.AddDbContext<FieldProDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// ===========================
// CORS (aperto per ora)
// ===========================
const string CorsPolicyName = "FrontendCors";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: CorsPolicyName, policy =>
    {
        policy
            .AllowAnyOrigin()
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
    var scheduledUtc = DateTime.SpecifyKind(request.ScheduledAt, DateTimeKind.Utc);

    var job = new Job
    {
        Code = request.Code,
        CustomerName = request.CustomerName,
        Address = request.Address,
        ScheduledAt = scheduledUtc,
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
