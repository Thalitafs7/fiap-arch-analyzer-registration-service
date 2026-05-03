using Application.DTOs;
using MediatR;

namespace Application.Commands.UpdateAnalise;

public record UpdateAnaliseCommand(
    Guid ClienteId,
    Guid Id,
    string? Descricao,
    string Nome    

) : IRequest<AnaliseDto>;
