using MediatR;

namespace FieldPro.Application.Jobs.Commands.BulkDeleteJobs;

public class BulkDeleteJobsCommand : IRequest<bool>
{
    public List<int> JobIds { get; set; } = new();
}
