using CrossService.IntegrationTests.Contracts.DTOs;

namespace CrossService.IntegrationTests.Builders;

public class ClienteBuilder
{
    private string _nome = $"Cliente Teste {Guid.NewGuid().ToString("N")[..8]}";
    private string _documento = GerarCpfValido();
    private TipoCliente _tipoCliente = TipoCliente.Fisica;
    private Sexo? _sexo = Sexo.Masculino;
    private DateTime? _dataNascimento = null;

    public static ClienteBuilder Padrao() => new();

    public ClienteBuilder ComNome(string nome) { _nome = nome; return this; }
    public ClienteBuilder ComDocumento(string documento) { _documento = documento; return this; }
    public ClienteBuilder PessoaJuridica() { _tipoCliente = TipoCliente.Juridica; _documento = GerarCnpjValido(); return this; }
    public ClienteBuilder ComSexo(Sexo sexo) { _sexo = sexo; return this; }
    public ClienteBuilder ComDataNascimento(DateTime data) { _dataNascimento = data; return this; }

    public CriarClienteRequest Build() => new(
        _nome,
        _documento,
        _tipoCliente,
        _sexo,
        _dataNascimento);

    private static string GerarCpfValido()
    {
        var random = new Random();
        var cpf = new int[11];

        for (int i = 0; i < 9; i++)
            cpf[i] = random.Next(0, 10);

        // Calcular primeiro dígito verificador
        var soma = 0;
        for (int i = 0; i < 9; i++)
            soma += cpf[i] * (10 - i);
        var resto = soma % 11;
        cpf[9] = resto < 2 ? 0 : 11 - resto;

        // Calcular segundo dígito verificador
        soma = 0;
        for (int i = 0; i < 10; i++)
            soma += cpf[i] * (11 - i);
        resto = soma % 11;
        cpf[10] = resto < 2 ? 0 : 11 - resto;

        return string.Join("", cpf);
    }

    private static string GerarCnpjValido()
    {
        var random = new Random();
        var cnpj = new int[14];

        for (int i = 0; i < 12; i++)
            cnpj[i] = random.Next(0, 10);

        // Calcular primeiro dígito verificador
        var multiplicador1 = new int[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var soma = 0;
        for (int i = 0; i < 12; i++)
            soma += cnpj[i] * multiplicador1[i];
        var resto = soma % 11;
        cnpj[12] = resto < 2 ? 0 : 11 - resto;

        // Calcular segundo dígito verificador
        var multiplicador2 = new int[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        soma = 0;
        for (int i = 0; i < 13; i++)
            soma += cnpj[i] * multiplicador2[i];
        resto = soma % 11;
        cnpj[13] = resto < 2 ? 0 : 11 - resto;

        return string.Join("", cnpj);
    }
}
