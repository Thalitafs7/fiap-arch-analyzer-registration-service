using CrossService.IntegrationTests.Common;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CrossService.IntegrationTests.Scenarios.Security;

[Collection("IntegrationTests")]
public class TC14_AutenticacaoJWTTests : IntegrationTestBase
{
    public TC14_AutenticacaoJWTTests(TestContainersFixture containers)
        : base(containers) { }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "High")]
    public async Task Endpoint_SemToken_DeveRetornar401()
    {
        // Arrange - Criar client sem token
        var clientSemToken = new HttpClient
        {
            BaseAddress = OrdensHttpClient.BaseAddress
        };

        // Act
        var response = await clientSemToken.GetAsync("/api/ordens");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "High")]
    public async Task Endpoint_ComTokenInvalido_DeveRetornar401()
    {
        // Arrange
        var clientTokenInvalido = new HttpClient
        {
            BaseAddress = OrdensHttpClient.BaseAddress
        };
        clientTokenInvalido.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "token.invalido.aqui");

        // Act
        var response = await clientTokenInvalido.GetAsync("/api/ordens");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "High")]
    public async Task Endpoint_ComTokenValido_DeveRetornar200()
    {
        // Arrange - OrdensClient já tem token válido configurado

        // Act
        var response = await OrdensHttpClient.GetAsync("/api/ordens");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "High")]
    public async Task Login_ComCredenciaisValidas_DeveRetornarToken()
    {
        // Arrange
        var loginRequest = new
        {
            Email = "test.jwt@example.com",
            Senha = "Test@123"
        };

        // Primeiro registrar o usuário
        await CadastrosHttpClient.PostAsJsonAsync("/api/auth/registrar", new
        {
            Nome = "Test JWT User",
            Email = loginRequest.Email,
            Senha = loginRequest.Senha,
            Tipo = "Admin"
        });

        // Act
        var response = await CadastrosHttpClient.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("token");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "High")]
    public async Task Login_ComCredenciaisInvalidas_DeveRetornar401()
    {
        // Arrange
        var loginRequest = new
        {
            Email = "naoexiste@example.com",
            Senha = "SenhaErrada123"
        };

        // Act
        var loginResponse = await CadastrosHttpClient.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        loginResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "Medium")]
    public async Task TokenExpirado_DeveRetornar401()
    {
        // Arrange - Gerar token com expiração no passado
        var tokenExpirado = JwtTokenGenerator.GerarTokenExpirado();

        var clientTokenExpirado = new HttpClient
        {
            BaseAddress = OrdensHttpClient.BaseAddress
        };
        clientTokenExpirado.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenExpirado);

        // Act
        var response = await clientTokenExpirado.GetAsync("/api/ordens");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
