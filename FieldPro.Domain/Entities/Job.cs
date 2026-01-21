using System.ComponentModel.DataAnnotations;

namespace FieldPro.Domain.Entities;

public class Job
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;
    public string? Notes { get; set; }


    [Required]
    [StringLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    public DateTime ScheduledAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Scheduled"; // Scheduled, InProgress, Completed

    public string? Project { get; set; }

    public int? TechnicianId { get; set; }
    public Technician? Technician { get; set; }

    // Soft delete / archivio
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
