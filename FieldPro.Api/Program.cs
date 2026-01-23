using FieldPro.Domain.Entities;
using FieldPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FieldPro.Application.Tenancy;
using FieldPro.Api.Infrastructure.Tenancy;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("=== FieldPro API BOOT v3 ===");

// ===========================
// CORS (frontend ufficiali)
// ===========================
const string CorsPolicyName = "AllowFrontend";

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://fieldpro-solution.vercel.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

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

// Scegli quale usare
var env = builder.Environment.EnvironmentName;
var connectionString = string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase)
    ? localConnectionString
    : renderConnectionString;

Console.WriteLine($"[DEBUG] Using connection string: {connectionString}");

builder.Services.AddDbContext<FieldProDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// Tenancy / HttpContext
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, HttpTenantProvider>();

// ===========================
// App
// ===========================
var app = builder.Build();

app.UseCors(CorsPolicyName);

// Root
app.MapGet("/", () => "FieldPro API");

// ===========================
// Technicians (tenant-based)
// ===========================
app.MapGet("/technicians", async (FieldProDbContext db, ITenantProvider tenantProvider) =>
{
    var tenantId = tenantProvider.TenantId;

    var technicians = await db.Technicians
        .Where(t => t.TenantId == tenantId)
        .OrderBy(t => t.Name)
        .ToListAsync();

    return Results.Ok(technicians);
});

// ===========================
// Jobs endpoints
// ===========================

// GET jobs (paginato + filtri + includeArchived + tenant)
app.MapGet("/jobs", async (
    FieldProDbContext db,
    ITenantProvider tenantProvider,
    int page = 1,
    int pageSize = 20,
    string? status = null,
    string? search = null,
    bool includeArchived = false
) =>
{
    var tenantId = tenantProvider.TenantId;

    var query = db.Jobs
        .Include(j => j.Technician)
        .Where(j => j.TenantId == tenantId)
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
app.MapPost("/jobs", async (
    FieldProDbContext db,
    ITenantProvider tenantProvider,
    JobCreateRequest request
) =>
{
    var tenantId = tenantProvider.TenantId;

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
        DeletedAt = null,
        TenantId = tenantId
    };

    db.Jobs.Add(job);
    await db.SaveChangesAsync();

    return Results.Created($"/jobs/{job.Id}", job);
});

// PUT status/notes
app.MapPut("/jobs/{id:int}", async (
    FieldProDbContext db,
    ITenantProvider tenantProvider,
    int id,
    JobUpdateStatusRequest request
) =>
{
    var tenantId = tenantProvider.TenantId;

    var job = await db.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.TenantId == tenantId);
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

// DELETE job -> soft delete singolo
app.MapDelete("/jobs/{id:int}", async (
    FieldProDbContext db,
    ITenantProvider tenantProvider,
    int id
) =>
{
    var tenantId = tenantProvider.TenantId;

    var job = await db.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.TenantId == tenantId);
    if (job == null)
    {
        return Results.NotFound();
    }

    job.IsDeleted = true;
    job.DeletedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

// DELETE job -> hard delete definitivo
app.MapDelete("/jobs/{id:int}/hard", async (
    FieldProDbContext db,
    ITenantProvider tenantProvider,
    int id
) =>
{
    var tenantId = tenantProvider.TenantId;

    var job = await db.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.TenantId == tenantId);
    if (job == null)
    {
        return Results.NotFound();
    }

    db.Jobs.Remove(job);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

// POST bulk-delete jobs -> soft delete multiplo
app.MapPost("/jobs/bulk-delete", async (
    FieldProDbContext db,
    ITenantProvider tenantProvider,
    BulkDeleteJobsRequest request
) =>
{
    var tenantId = tenantProvider.TenantId;

    if (request.JobIds == null || request.JobIds.Count == 0)
    {
        return Results.BadRequest("Nessun jobId specificato");
    }

    var updated = await db.Jobs
        .Where(j => request.JobIds.Contains(j.Id) && j.TenantId == tenantId)
        .ExecuteUpdateAsync(setters => setters
            .SetProperty(j => j.IsDeleted, true)
            .SetProperty(j => j.DeletedAt, DateTime.UtcNow));

    return updated > 0 ? Results.NoContent() : Results.BadRequest("Nessun job trovato");
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

public record BulkDeleteJobsRequest(List<int> JobIds);
