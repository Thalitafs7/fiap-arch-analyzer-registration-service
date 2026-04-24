using Application.DTOs;
using MediatR;

namespace Application.Queries.ObterTodasOrdens;

public record ObterTodasOrdensQuery : IRequest<IEnumerable<AnaliseDto>>;
