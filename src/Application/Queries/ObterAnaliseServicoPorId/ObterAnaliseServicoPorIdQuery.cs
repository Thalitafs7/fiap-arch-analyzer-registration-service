using Application.DTOs;
using MediatR;

namespace Application.Queries.ObterAnaliseServicoPorId;

public record ObterAnaliseServicoPorIdQuery(Guid Id) : IRequest<AnaliseDto?>;
