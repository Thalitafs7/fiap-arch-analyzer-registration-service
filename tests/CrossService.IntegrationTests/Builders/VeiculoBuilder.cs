using CrossService.IntegrationTests.Contracts.DTOs;

namespace CrossService.IntegrationTests.Builders;

public class VeiculoBuilder
{
    private Guid _clienteId = Guid.Empty;
    private string _placa = GerarPlaca();
    private string _marca = "Toyota";
    private string _modelo = "Corolla";
    private int _ano = 2023;
    private string? _cor = "Prata";

    public static VeiculoBuilder Padrao() => new();

    public VeiculoBuilder ParaCliente(Guid clienteId) { _clienteId = clienteId; return this; }
    public VeiculoBuilder ComPlaca(string placa) { _placa = placa; return this; }
    public VeiculoBuilder ComMarca(string marca) { _marca = marca; return this; }
    public VeiculoBuilder ComModelo(string modelo) { _modelo = modelo; return this; }
    public VeiculoBuilder ComAno(int ano) { _ano = ano; return this; }
    public VeiculoBuilder ComCor(string cor) { _cor = cor; return this; }

    public CriarVeiculoRequest Build() => new(
        _clienteId,
        _placa,
        _marca,
        _modelo,
        _ano,
        _cor);

    private static string GerarPlaca()
    {
        var random = new Random();
        var letras = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        // Formato antigo: ABC1234 (3 letras + 4 números)
        var placa = "";
        for (int i = 0; i < 3; i++)
            placa += letras[random.Next(letras.Length)];
        placa += random.Next(1000, 9999).ToString();
        return placa;
    }
}
