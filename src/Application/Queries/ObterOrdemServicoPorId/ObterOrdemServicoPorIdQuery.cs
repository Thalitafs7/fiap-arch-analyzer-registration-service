using Application.DTOs;
using MediatR;

namespace Application.Queries.ObterOrdemServicoPorId;

public record ObterOrdemServicoPorIdQuery(Guid Id) : IRequest<AnaliseDto?>;
