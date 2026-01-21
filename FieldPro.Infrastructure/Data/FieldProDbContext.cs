using FieldPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FieldPro.Infrastructure.Data;

public class FieldProDbContext : DbContext
{
    public FieldProDbContext(DbContextOptions<FieldProDbContext> options)
        : base(options)
    {
    }

    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Technician> Technicians => Set<Technician>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(j => j.Id);

            entity.Property(j => j.Code)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(j => j.CustomerName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(j => j.Address)
                .IsRequired();

            entity.Property(j => j.Status)
                .IsRequired()
                .HasMaxLength(50);

            // Soft delete: nessun vincolo speciale, solo colonne normali
            entity.Property(j => j.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(j => j.DeletedAt);

            entity.HasOne(j => j.Technician)
                .WithMany()
                .HasForeignKey(j => j.TechnicianId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Technician>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(t => t.Email);
        });
    }
}
