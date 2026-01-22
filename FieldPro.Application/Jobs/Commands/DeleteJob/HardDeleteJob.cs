using MediatR;

namespace FieldPro.Application.Jobs.Commands.HardDeleteJob;

public record HardDeleteJobCommand(int Id) : IRequest<bool>;
