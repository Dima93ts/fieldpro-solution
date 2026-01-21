namespace FieldPro.Api.Models;

public class Job
{
    public int Id { get; set; }
    public string Code { get; set; } = default!;
    public string CustomerName { get; set; } = default!;
    public string Address { get; set; } = default!;
    public DateTime ScheduledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "Scheduled";
}
