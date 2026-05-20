using Application.DTOs;
using MediatR;

namespace Application.Commands.AtualizarStatusPorSoatId;

public record AtualizarStatusPorSoatIdCommand(Guid SoatAnalysisId, string Status) : IRequest<AnaliseDto>;
