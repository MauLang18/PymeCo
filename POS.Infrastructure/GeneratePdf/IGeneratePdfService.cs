namespace POS.Infrastructure.GeneratePdf;

public interface IGeneratePdfService
{
    byte[] GeneratePdf(string htmlContent);
}
