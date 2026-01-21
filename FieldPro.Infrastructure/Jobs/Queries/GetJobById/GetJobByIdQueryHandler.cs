using FieldPro.Application.Jobs.DTOs;
using FieldPro.Application.Jobs.Queries.GetJobById;
using FieldPro.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FieldPro.Infrastructure.Jobs.Queries.GetJobById;

public class GetJobByIdQueryHandler : IRequestHandler<GetJobByIdQuery, JobDto?>
{
    private readonly FieldProDbContext _context;

    public GetJobByIdQueryHandler(FieldProDbContext context)
    {
        _context = context;
    }

    public async Task<JobDto?> Handle(GetJobByIdQuery request, CancellationToken cancellationToken)
    {
        var job = await _context.Jobs
            .Include(j => j.Technician)
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);

        if (job == null)
            return null;

        return new JobDto
        {
            Id = job.Id,
            Code = job.Code,
            CustomerName = job.CustomerName,
            Address = job.Address,
            ScheduledAt = job.ScheduledAt,
            CompletedAt = job.CompletedAt,
            Status = job.Status,
            Project = job.Project,
            TechnicianId = job.TechnicianId,
            TechnicianName = job.Technician?.Name
        };
    }
}
