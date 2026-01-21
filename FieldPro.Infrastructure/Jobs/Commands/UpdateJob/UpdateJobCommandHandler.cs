using FieldPro.Application.Jobs.Commands.UpdateJob;
using FieldPro.Infrastructure.Data;
using MediatR;

namespace FieldPro.Infrastructure.Jobs.Commands.UpdateJob;

public class UpdateJobCommandHandler : IRequestHandler<UpdateJobCommand>
{
    private readonly FieldProDbContext _context;

    public UpdateJobCommandHandler(FieldProDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _context.Jobs.FindAsync(request.Id);
        if (job == null)
        {
            throw new Exception("Job non trovato");
        }

        if (request.Code != null)
            job.Code = request.Code;

        if (request.CustomerName != null)
            job.CustomerName = request.CustomerName;

        if (request.Address != null)
            job.Address = request.Address;

        if (request.ScheduledAt.HasValue)
            job.ScheduledAt = request.ScheduledAt.Value;

        if (request.CompletedAt.HasValue)
            job.CompletedAt = request.CompletedAt.Value;

        if (request.Status != null)
            job.Status = request.Status;

        if (request.Project != null)
            job.Project = request.Project;
            
            if (request.Notes != null)
{
    job.Notes = request.Notes;
}


        job.TechnicianId = request.TechnicianId;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
