using FieldPro.Application.Technicians.DTOs;
using MediatR;

namespace FieldPro.Application.Technicians.Queries.GetTechnicians;

public class GetTechniciansQuery : IRequest<List<TechnicianDto>>
{
}
