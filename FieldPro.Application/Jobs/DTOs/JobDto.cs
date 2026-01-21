// FieldPro.Application/Jobs/DTOs/JobDto.cs
namespace FieldPro.Application.Jobs.DTOs;

public class JobDto
{
    public int Id { get; set; }
    public string Code { get; set; } = default!;
    public string CustomerName { get; set; } = default!;
    public string Address { get; set; } = default!;
    public DateTime ScheduledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = default!;
    public string? Project { get; set; }
    public int? TechnicianId { get; set; }
    public string? TechnicianName { get; set; }

    public string? Notes { get; set; }  // deve esserci
}
