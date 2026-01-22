using FieldPro.Application.Jobs.DTOs;
using FieldPro.Application.Jobs.Queries.GetJobs;
using FieldPro.Application.Tenancy;
using FieldPro.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FieldPro.Infrastructure.Jobs.Queries.GetJobs;

public class GetJobsQueryHandler : IRequestHandler<GetJobsQuery, List<JobDto>>
{
    private readonly FieldProDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public GetJobsQueryHandler(FieldProDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<List<JobDto>> Handle(GetJobsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.TenantId;


        var query = _context.Jobs
            .Include(j => j.Technician)
            .Where(j => j.TenantId == tenantId)   // 🔹 filtro per tenant
            .AsQueryable();

        if (!request.IncludeArchived)
        {
            query = query.Where(j => !j.IsDeleted);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(j => j.Status == request.Status);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(j =>
                j.Code.Contains(request.Search) ||
                j.CustomerName.Contains(request.Search) ||
                (j.Project != null && j.Project.Contains(request.Search)));
        }

        var items = await query
            .OrderByDescending(j => j.ScheduledAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(j => new JobDto
            {
                Id = j.Id,
                Code = j.Code,
                CustomerName = j.CustomerName,
                Address = j.Address,
                ScheduledAt = j.ScheduledAt,
                CompletedAt = j.CompletedAt,
                Status = j.Status,
                Project = j.Project,
                TechnicianId = j.TechnicianId,
                TechnicianName = j.Technician != null ? j.Technician.Name : null,
                Notes = j.Notes
            })
            .ToListAsync(cancellationToken);

        return items;
    }
}
