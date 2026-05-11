using System.Text.Json.Serialization;

namespace CrossService.IntegrationTests.Contracts.DTOs;

public class CriarChecklistDto
{
    public Guid OrdemServicoId { get; set; }
    public Guid VeiculoId { get; set; }
    public Guid? ClienteId { get; set; }
    public VeiculoSnapshotDto Veiculo { get; set; } = null!;
    public string TipoInspecao { get; set; } = null!;
    public Guid InspetorId { get; set; }
    public string InspetorNome { get; set; } = null!;
    public int Quilometragem { get; set; }
    public int? DiasParaExpirar { get; set; }
    public List<CategoriaChecklistDto>? Categorias { get; set; }
}

public class VeiculoSnapshotDto
{
    public string Placa { get; set; } = null!;
    public string Marca { get; set; } = null!;
    public string Modelo { get; set; } = null!;
    public int Ano { get; set; }
    public string? Cor { get; set; }
}

public class CategoriaChecklistDto
{
    public string Nome { get; set; } = null!;
    public int Ordem { get; set; }
    public List<ItemChecklistDto>? Itens { get; set; }
}

public class ItemChecklistDto
{
    public string Codigo { get; set; } = null!;
    public string Descricao { get; set; } = null!;
    public StatusItemInspecao Status { get; set; }
    public string? Observacao { get; set; }
    public string? Recomendacao { get; set; }
    public AcaoRequerida? Acao { get; set; }
    public List<string>? Fotos { get; set; }
    public MedicaoItemDto? Medicao { get; set; }
}

public class MedicaoItemDto
{
    public decimal Valor { get; set; }
    public string Unidade { get; set; } = null!;
    public decimal? Minimo { get; set; }
    public decimal? Maximo { get; set; }
}

public class ChecklistResponseDto
{
    public Guid Id { get; set; }
    public Guid OrdemServicoId { get; set; }
    public Guid VeiculoId { get; set; }
    public Guid? ClienteId { get; set; }
    public VeiculoSnapshotDto Veiculo { get; set; } = null!;
    public string TipoInspecao { get; set; } = null!;
    public DateTime DataInspecao { get; set; }
    public Guid InspetorId { get; set; }
    public string InspetorNome { get; set; } = null!;
    public int Quilometragem { get; set; }
    public List<CategoriaChecklistDto>? Categorias { get; set; }
    public ResultadoInspecaoDto Resultado { get; set; } = null!;
    public string? AssinaturaCliente { get; set; }
    public string? AssinaturaInspetor { get; set; }
}

public class ResultadoInspecaoDto
{
    public int TotalItens { get; set; }
    public int ItensOk { get; set; }
    public int ItensAtencao { get; set; }
    public int ItensCriticos { get; set; }
    public StatusInspecao StatusGeral { get; set; }
}

public class ProblemaFrequenteDto
{
    public string Categoria { get; set; } = null!;
    public string Item { get; set; } = null!;
    public int Total { get; set; }
    public int Criticos { get; set; }
}

public class EstatisticasInspecaoDto
{
    public int TotalInspecoes { get; set; }
    public int Aprovados { get; set; }
    public int AprovadosComRessalvas { get; set; }
    public int Reprovados { get; set; }
}

public class OutboxMessageDto
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public DateTime? ProcessedAt { get; set; }
    public int RetryCount { get; set; }
    public string? Error { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime DataCadastro { get; set; }
}

public enum StatusItemInspecao
{
    NaoVerificado = 0,
    Ok = 1,
    Atencao = 2,
    Critico = 3
}

public enum AcaoRequerida
{
    Nenhuma = 0,
    Monitorar = 1,
    AgendarTroca = 2,
    SubstituirImediato = 3
}

public enum StatusInspecao
{
    Pendente = 0,
    Aprovado = 1,
    AprovadoComRessalvas = 2,
    Reprovado = 3
}
