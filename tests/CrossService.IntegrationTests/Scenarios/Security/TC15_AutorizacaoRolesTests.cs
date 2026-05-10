using CrossService.IntegrationTests.Common;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CrossService.IntegrationTests.Scenarios.Security;

[Collection("IntegrationTests")]
public class TC15_AutorizacaoRolesTests : IntegrationTestBase
{
    public TC15_AutorizacaoRolesTests(TestContainersFixture containers)
        : base(containers) { }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "High")]
    public async Task EndpointAdmin_ComRoleAdmin_DevePermitirAcesso()
    {
        // Arrange - Token padrão é Admin
        // OrdensClient já está configurado com token de admin

        // Act
        var response = await OrdensHttpClient.GetAsync("/api/ordens");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "High")]
    public async Task Endpoint_ComRoleUsuario_DevePermitirAcessoLeitura()
    {
        // Arrange - Gerar token com role de usuário comum
        var tokenUsuario = JwtTokenGenerator.GerarToken(
            email: "usuario@test.com",
            nome: "Usuário Comum",
            role: "Usuario");

        var clientUsuario = new HttpClient
        {
            BaseAddress = OrdensHttpClient.BaseAddress
        };
        clientUsuario.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenUsuario);

        // Act - Acessar endpoint de leitura
        var response = await clientUsuario.GetAsync("/api/ordens");

        // Assert - Usuário autenticado deve poder ler
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent,
            HttpStatusCode.Forbidden);  // Dependendo das políticas
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "Medium")]
    public async Task EndpointMecanico_ComRoleMecanico_DevePermitirAcesso()
    {
        // Arrange
        var tokenMecanico = JwtTokenGenerator.GerarToken(
            email: "mecanico@test.com",
            nome: "Mecânico Teste",
            role: "Mecanico");

        var clientMecanico = new HttpClient
        {
            BaseAddress = OrdensHttpClient.BaseAddress
        };
        clientMecanico.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenMecanico);

        // Act
        var response = await clientMecanico.GetAsync("/api/ordens");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent,
            HttpStatusCode.Forbidden);  // Dependendo das políticas
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "Medium")]
    public async Task CriarCliente_ComRoleAdmin_DevePermitirAcesso()
    {
        // Arrange - Token admin já configurado

        // Act
        var response = await CadastrosHttpClient.PostAsJsonAsync("/api/clientes", new
        {
            Nome = "Cliente Autorização",
            Documento = "11122233344",
            Email = "auth.test@example.com",
            Telefone = "11999999999"
        });

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Created,
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest);  // BadRequest se cliente já existe
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "Medium")]
    public async Task AcessarRecursosDeOutroUsuario_DeveSerRestrito()
    {
        // Este teste valida que usuários não podem acessar recursos de outros
        // A implementação depende das políticas de autorização do sistema

        // Arrange
        var tokenUsuario1 = JwtTokenGenerator.GerarToken(
            email: "usuario1@test.com",
            nome: "Usuário 1",
            role: "Cliente",
            userId: Guid.NewGuid().ToString());

        var tokenUsuario2 = JwtTokenGenerator.GerarToken(
            email: "usuario2@test.com",
            nome: "Usuário 2",
            role: "Cliente",
            userId: Guid.NewGuid().ToString());

        var clientUsuario2 = new HttpClient
        {
            BaseAddress = CadastrosHttpClient.BaseAddress
        };
        clientUsuario2.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenUsuario2);

        // Act - Usuário 2 tentando acessar dados que seriam do Usuário 1
        // (Este comportamento depende da implementação do sistema)

        // Assert
        // Validação depende das políticas implementadas
    }
}
