using System.ComponentModel.DataAnnotations;

namespace FieldPro.Domain.Entities;

public class Technician
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Multi-tenant
    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;
}
