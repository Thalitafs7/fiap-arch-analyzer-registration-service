namespace CrossService.IntegrationTests.Contracts.DTOs;

public record VeiculoDto(
    Guid Id,
    Guid ClienteId,
    string Placa,
    string Marca,
    string Modelo,
    int Ano,
    string? Cor);

public record CriarVeiculoRequest(
    Guid ClienteId,
    string Placa,
    string Marca,
    string Modelo,
    int Ano,
    string? Cor);
