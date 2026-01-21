using FieldPro.Application.Jobs.DTOs;
using MediatR;

namespace FieldPro.Application.Jobs.Queries.GetJobs;

public class GetJobsQuery : IRequest<List<JobDto>>
{
    public string? Status { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    // Nuovo: permette di includere job archiviati
    public bool IncludeArchived { get; set; } = false;
}
