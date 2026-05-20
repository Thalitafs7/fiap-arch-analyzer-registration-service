using Application.DTOs;
using MediatR;

namespace Application.Commands.RefreshStatusAnalise;

public record RefreshStatusAnaliseCommand(Guid AnaliseId) : IRequest<AnaliseDto?>;
