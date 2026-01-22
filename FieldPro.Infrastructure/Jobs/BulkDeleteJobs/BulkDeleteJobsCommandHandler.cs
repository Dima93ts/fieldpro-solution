using FieldPro.Application.Jobs.Commands.BulkDeleteJobs;
using FieldPro.Application.Tenancy;
using FieldPro.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FieldPro.Infrastructure.Jobs.Commands.BulkDeleteJobs;

public class BulkDeleteJobsCommandHandler : IRequestHandler<BulkDeleteJobsCommand, bool>
{
    private readonly FieldProDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public BulkDeleteJobsCommandHandler(FieldProDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<bool> Handle(BulkDeleteJobsCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.TenantId;
        var jobIds = request.JobIds;

        var updated = await _context.Jobs
            .Where(j => jobIds.Contains(j.Id) && j.TenantId == tenantId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(j => j.IsDeleted, true)
                .SetProperty(j => j.DeletedAt, DateTime.UtcNow),
                cancellationToken);

        return updated > 0;
    }
}
