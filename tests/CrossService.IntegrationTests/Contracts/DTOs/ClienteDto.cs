namespace CrossService.IntegrationTests.Contracts.DTOs;

public record ClienteDto(
    Guid Id,
    string Nome,
    string Documento,
    TipoCliente TipoCliente,
    Sexo? Sexo,
    DateTime? DataNascimento);

public record CriarClienteRequest(
    string Nome,
    string Documento,
    TipoCliente TipoCliente,
    Sexo? Sexo,
    DateTime? DataNascimento);

public enum TipoCliente
{
    Fisica = 0,
    Juridica = 1
}

public enum Sexo
{
    Masculino = 0,
    Feminino = 1
}
