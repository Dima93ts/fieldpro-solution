using FieldPro.Application.Jobs.Commands.HardDeleteJob;
using FieldPro.Infrastructure.Data;
using MediatR;

namespace FieldPro.Infrastructure.Jobs.Commands.HardDeleteJob;

public class HardDeleteJobCommandHandler : IRequestHandler<HardDeleteJobCommand, bool>
{
    private readonly FieldProDbContext _context;

    public HardDeleteJobCommandHandler(FieldProDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(HardDeleteJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _context.Jobs.FindAsync(request.Id);
        if (job == null)
            return false;

        _context.Jobs.Remove(job);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}