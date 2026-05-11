using Application.DTOs;
using MediatR;

namespace Application.Commands.AtualizarStatusAnalise;

public record AtualizarStatusAnaliseCommand(Guid Id) : IRequest<AnaliseDto>;
