using MediatR;

namespace FieldPro.Application.Jobs.Commands.UpdateJob;

public class UpdateJobCommand : IRequest
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? CustomerName { get; set; }
    public string? Address { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Status { get; set; }
    public string? Project { get; set; }
    public int? TechnicianId { get; set; }

    public string? Notes { get; set; } // nuovo
}
