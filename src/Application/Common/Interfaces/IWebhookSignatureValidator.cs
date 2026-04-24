namespace Application.Common.Interfaces;

public interface IWebhookSignatureValidator
{
    bool ValidateSignature(string xSignature, string xRequestId, string dataId, string secret);
}
