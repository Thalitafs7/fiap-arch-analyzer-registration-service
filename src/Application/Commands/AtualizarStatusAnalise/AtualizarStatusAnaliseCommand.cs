using Application.DTOs;
using MediatR;

namespace Application.Commands.AtualizarStatusAnalise;

public record AtualizarStatusAnaliseCommand(Guid Id, Guid? SoatAnalysisId, string? Status = null) : IRequest<AnaliseDto>;
