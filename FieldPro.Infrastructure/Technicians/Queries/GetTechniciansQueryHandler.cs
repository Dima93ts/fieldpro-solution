using FieldPro.Application.Technicians.DTOs;
using FieldPro.Application.Technicians.Queries.GetTechnicians;
using FieldPro.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FieldPro.Infrastructure.Technicians.Queries.GetTechnicians;

public class GetTechniciansQueryHandler : IRequestHandler<GetTechniciansQuery, List<TechnicianDto>>
{
    private readonly FieldProDbContext _context;

    public GetTechniciansQueryHandler(FieldProDbContext context)
    {
        _context = context;
    }

    public async Task<List<TechnicianDto>> Handle(GetTechniciansQuery request, CancellationToken cancellationToken)
    {
        return await _context.Technicians
            .OrderBy(t => t.Name)
            .Select(t => new TechnicianDto
            {
                Id = t.Id,
                Name = t.Name,
                Email = t.Email
            })
            .ToListAsync(cancellationToken);
    }
}
