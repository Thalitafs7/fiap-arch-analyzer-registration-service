namespace Domain.Enums;

public enum StatusAnalise
{
    Recebido = 0,
    EmProcessamento = 1,
    Analisado = 2,
    Error = 3
}

public static class StatusAnaliseExtensions
{
    public static StatusAnalise FromExternalStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return StatusAnalise.Error;

        return status.Trim().ToUpperInvariant() switch
        {
            "RECEIVED" or "RECEBIDO" => StatusAnalise.Recebido,
            "PROCESSING" or "EMPROCESSAMENTO" or "EM_PROCESSAMENTO" => StatusAnalise.EmProcessamento,
            "ANALYZED" or "ANALISADO" => StatusAnalise.Analisado,
            "ERROR" or "ERRO" => StatusAnalise.Error,
            _ => StatusAnalise.Error
        };
    }
}
