using FieldPro.Application.Jobs.Commands.CreateJob;
using FieldPro.Domain.Entities;
using FieldPro.Infrastructure.Data;
using MediatR;
using FieldPro.Application.Tenancy;


namespace FieldPro.Infrastructure.Jobs.Commands.CreateJob;

public class CreateJobCommandHandler : IRequestHandler<CreateJobCommand, int>
{
    private readonly FieldProDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public CreateJobCommandHandler(FieldProDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<int> Handle(CreateJobCommand request, CancellationToken cancellationToken)
{
    // ScheduledAt: assicuriamoci che sia trattato come UTC
    var scheduledUtc = request.ScheduledAt.Kind == DateTimeKind.Unspecified
        ? DateTime.SpecifyKind(request.ScheduledAt, DateTimeKind.Utc)
        : request.ScheduledAt.ToUniversalTime();

    DateTime? completedUtc = null;
    // per ora resta null, quando userai CompletedAt lo gestirai qui

    var tenantId = _tenantProvider.GetCurrentTenantId();

    var job = new Job
    {
        Code = request.Code,
        CustomerName = request.CustomerName,
        Address = request.Address,
        ScheduledAt = scheduledUtc,
        CompletedAt = completedUtc,
        Status = request.Status,
        Project = request.Project,
        TechnicianId = request.TechnicianId,
        TenantId = tenantId,
        IsDeleted = false
    };

    _context.Jobs.Add(job);
    await _context.SaveChangesAsync(cancellationToken);

    return job.Id;
}

}
