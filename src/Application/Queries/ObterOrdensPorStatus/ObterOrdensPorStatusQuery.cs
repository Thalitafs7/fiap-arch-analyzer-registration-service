using Application.DTOs;
using MediatR;

namespace Application.Queries.ObterOrdensPorStatus;

public record ObterOrdensPorStatusQuery(AnaliseDto Status) : IRequest<IEnumerable<AnaliseDto>>;
