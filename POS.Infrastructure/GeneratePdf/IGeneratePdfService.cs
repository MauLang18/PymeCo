namespace POS.Infrastructure.GeneratePdf;

public interface IGeneratePdfService
{
    byte[] GeneratePdf<T>(string htmlTemplate, T data);
}
