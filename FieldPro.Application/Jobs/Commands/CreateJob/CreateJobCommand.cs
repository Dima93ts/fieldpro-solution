using MediatR;

namespace FieldPro.Application.Jobs.Commands.CreateJob;

public class CreateJobCommand : IRequest<int>
{
    public string Code { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public string Status { get; set; } = "Scheduled";
    public string? Project { get; set; }
    public int? TechnicianId { get; set; }
}
