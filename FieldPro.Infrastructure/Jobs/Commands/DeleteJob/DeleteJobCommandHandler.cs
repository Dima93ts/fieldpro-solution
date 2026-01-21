using FieldPro.Application.Jobs.Commands.DeleteJob;
using FieldPro.Infrastructure.Data;
using MediatR;

namespace FieldPro.Infrastructure.Jobs.Commands.DeleteJob;

public class DeleteJobCommandHandler : IRequestHandler<DeleteJobCommand, bool>
{
    private readonly FieldProDbContext _context;

    public DeleteJobCommandHandler(FieldProDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _context.Jobs.FindAsync(request.Id);
        if (job == null)
            return false;

        // Soft delete: non rimuoviamo il record, lo archiviamo
        job.IsDeleted = true;
        job.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
