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
            "RECEIVED" => StatusAnalise.Recebido,
            "PROCESSING" => StatusAnalise.EmProcessamento,
            "ANALYZED" => StatusAnalise.Analisado,
            "ERROR" => StatusAnalise.Error,
            _ => StatusAnalise.Error
        };
    }
}
