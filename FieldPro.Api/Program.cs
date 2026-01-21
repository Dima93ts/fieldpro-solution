using FieldPro.Domain.Entities;
using FieldPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Connection string PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Host=localhost;Port=5432;Database=fieldpro;Username=fieldpro;Password=fieldPro2026!";

builder.Services.AddDbContext<FieldProDbContext>(options =>
    options.UseNpgsql(connectionString));

// CORS: permetti frontend locali (dev + dist)
const string CorsPolicyName = "FrontendCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: CorsPolicyName, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173", // Vite dev
                "http://localhost:3000"  // dist servito con npx serve
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Middleware
app.UseCors(CorsPolicyName);

app.MapGet("/", () => "FieldPro API");

// GET tecnici
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

// DELETE job -> soft delete (IsDeleted/DeletedAt)
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

// DTO per richieste
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
