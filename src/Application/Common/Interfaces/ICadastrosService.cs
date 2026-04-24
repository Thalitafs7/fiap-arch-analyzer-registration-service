using Application.Common.DTOs;

namespace Application.Common.Interfaces;

public interface ICadastrosService
{
    Task<bool> ValidarClienteVeiculoAsync(Guid clienteId, Guid veiculoId, CancellationToken cancellationToken = default);
    Task<ClienteDto?> ObterClienteAsync(Guid clienteId, CancellationToken cancellationToken = default);

}
