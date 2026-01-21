using FieldPro.Application.Jobs.Commands.CreateJob;
using FieldPro.Domain.Entities;
using FieldPro.Infrastructure.Data;
using MediatR;

namespace FieldPro.Infrastructure.Jobs.Commands.CreateJob;

public class CreateJobCommandHandler : IRequestHandler<CreateJobCommand, int>
{
    private readonly FieldProDbContext _context;

    public CreateJobCommandHandler(FieldProDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateJobCommand request, CancellationToken cancellationToken)
    {
        // Normalizza la data a UTC per PostgreSQL (timestamp with time zone)
        var scheduledUtc = request.ScheduledAt.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(request.ScheduledAt, DateTimeKind.Utc)
            : request.ScheduledAt.ToUniversalTime();

        DateTime? completedUtc = null;
        if (request is { } && request.ScheduledAt != default && request.ScheduledAt != DateTime.MinValue)
        {
            // Se in futuro userai CompletedAt in CreateJobCommand, gestiscilo qui in modo simile
            // per ora rimane null
            completedUtc = null;
        }

        var job = new Job
        {
            Code = request.Code,
            CustomerName = request.CustomerName,
            Address = request.Address,
            ScheduledAt = scheduledUtc,
            CompletedAt = completedUtc,
            Status = request.Status,
            Project = request.Project,
            TechnicianId = request.TechnicianId
        };

        _context.Jobs.Add(job);
        await _context.SaveChangesAsync(cancellationToken);

        return job.Id;
    }
}
