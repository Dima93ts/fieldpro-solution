using FieldPro.Application.Jobs.DTOs;
using MediatR;

namespace FieldPro.Application.Jobs.Queries.GetJobById;

public class GetJobByIdQuery : IRequest<JobDto?>
{
    public int Id { get; }

    public GetJobByIdQuery(int id)
    {
        Id = id;
    }
}
