using Application.DTOs;
using MediatR;

namespace Application.Queries.ObterOrdensPorCliente;

public record ObterOrdensPorClienteQuery(Guid ClienteId) : IRequest<IEnumerable<AnaliseDto>>;
