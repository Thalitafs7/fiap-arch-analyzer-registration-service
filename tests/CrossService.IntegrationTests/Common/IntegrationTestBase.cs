using System.Net.Http.Headers;
using CrossService.IntegrationTests.Clients;
using MassTransit;
using Microsoft.Extensions.Configuration;

namespace CrossService.IntegrationTests.Common;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly TestContainersFixture Containers;
    protected readonly IConfiguration Configuration;
    protected HttpClient CadastrosHttpClient = null!;
    protected HttpClient OrdensHttpClient = null!;
    protected HttpClient EstoqueHttpClient = null!;
    protected CadastrosApiClient CadastrosApi = null!;
    protected OrdensApiClient OrdensApi = null!;
    protected EstoqueApiClient EstoqueApi = null!;
    private IBusControl? _bus;

    protected string CadastrosBaseUrl => Configuration["Services:MsCadastros:BaseUrl"] ?? "http://localhost:5001";
    protected string OrdensBaseUrl => Configuration["Services:MsOrdens:BaseUrl"] ?? "http://localhost:5002";
    protected string EstoqueBaseUrl => Configuration["Services:MsEstoque:BaseUrl"] ?? "http://localhost:5003";
    protected string RabbitMqHost => Configuration["RabbitMQ:Host"] ?? "localhost";
    protected int RabbitMqPort => int.Parse(Configuration["RabbitMQ:Port"] ?? "5672");

    protected IntegrationTestBase(TestContainersFixture containers)
    {
        Containers = containers;

        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    public virtual async Task InitializeAsync()
    {
        var token = JwtTokenGenerator.GenerateToken();

        CadastrosHttpClient = CreateHttpClient(CadastrosBaseUrl, token);
        OrdensHttpClient = CreateHttpClient(OrdensBaseUrl, token);
        EstoqueHttpClient = CreateHttpClient(EstoqueBaseUrl, token);

        CadastrosApi = new CadastrosApiClient(CadastrosHttpClient);
        OrdensApi = new OrdensApiClient(OrdensHttpClient);
        EstoqueApi = new EstoqueApiClient(EstoqueHttpClient);

        // Aguardar serviços estarem disponíveis
        await WaitForServicesAsync();
    }

    private async Task WaitForServicesAsync()
    {
        var timeout = TimeSpan.FromSeconds(60);
        var interval = TimeSpan.FromSeconds(2);
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < timeout)
        {
            try
            {
                var cadastrosOk = await CheckServiceHealthAsync(CadastrosHttpClient);
                var ordensOk = await CheckServiceHealthAsync(OrdensHttpClient);
                var estoqueOk = await CheckServiceHealthAsync(EstoqueHttpClient);

                if (cadastrosOk && ordensOk && estoqueOk)
                {
                    return;
                }
            }
            catch
            {
                // Ignorar erros durante a espera
            }

            await Task.Delay(interval);
        }

        throw new InvalidOperationException(
            $"Serviços não ficaram disponíveis em {timeout.TotalSeconds}s. " +
            $"Verifique se os containers estão rodando (docker-compose up).");
    }

    private async Task<bool> CheckServiceHealthAsync(HttpClient client)
    {
        try
        {
            // Tentar diferentes endpoints para verificar se o serviço está respondendo
            var endpoints = new[] { "/health", "/swagger/index.html", "/" };
            foreach (var endpoint in endpoints)
            {
                try
                {
                    var response = await client.GetAsync(endpoint);
                    if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return true; // Serviço está respondendo
                    }
                }
                catch { }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public virtual Task DisposeAsync()
    {
        CadastrosHttpClient?.Dispose();
        OrdensHttpClient?.Dispose();
        EstoqueHttpClient?.Dispose();
        _bus?.Stop();
        return Task.CompletedTask;
    }

    private static HttpClient CreateHttpClient(string baseUrl, string token)
    {
        var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client.Timeout = TimeSpan.FromSeconds(30);
        return client;
    }

    protected async Task<IBusControl> GetBusAsync()
    {
        if (_bus != null) return _bus;

        _bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
            cfg.Host(RabbitMqHost, (ushort)RabbitMqPort, "/", h =>
            {
                h.Username(Configuration["RabbitMQ:User"] ?? "guest");
                h.Password(Configuration["RabbitMQ:Password"] ?? "guest");
            });
        });

        await _bus.StartAsync();
        return _bus;
    }

    protected async Task PublishOrdemServicoCriadaEventAsync(Guid ordemServicoId, Guid clienteId, object[] insumos)
    {
        var bus = await GetBusAsync();
        await bus.Publish(new
        {
            OrdemServicoId = ordemServicoId,
            ClienteId = clienteId,
            Insumos = insumos,
            Timestamp = DateTime.UtcNow
        });
    }

    protected async Task PublishPagamentoAprovadoEventAsync(Guid ordemServicoId, Guid pagamentoId, decimal valor)
    {
        var bus = await GetBusAsync();
        await bus.Publish(new
        {
            OrdemServicoId = ordemServicoId,
            PagamentoId = pagamentoId,
            Valor = valor,
            Timestamp = DateTime.UtcNow
        });
    }

    protected async Task PublishOrcamentoCanceladoEventAsync(Guid ordemServicoId)
    {
        var bus = await GetBusAsync();
        await bus.Publish(new
        {
            OrdemServicoId = ordemServicoId,
            Timestamp = DateTime.UtcNow
        });
    }
}
