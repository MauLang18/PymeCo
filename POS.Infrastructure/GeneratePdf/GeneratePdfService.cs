using DinkToPdf;
using DinkToPdf.Contracts;

namespace POS.Infrastructure.GeneratePdf;

public class GeneratePdfService : IGeneratePdfService
{
    private readonly IConverter _pdfConverter;

    public GeneratePdfService(IConverter pdfConverter)
    {
        _pdfConverter = pdfConverter;
    }

    public byte[] GeneratePdf(string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
            throw new ArgumentException("HTML content cannot be null or empty.");

        var document = new HtmlToPdfDocument
        {
            GlobalSettings = new GlobalSettings
            {
                PaperSize = PaperKind.A4,
                Orientation = Orientation.Portrait,
            },
        };

        document.Objects.Add(
            new ObjectSettings
            {
                HtmlContent = htmlContent,
                WebSettings = new WebSettings { DefaultEncoding = "utf-8", LoadImages = true },
            }
        );

        return _pdfConverter.Convert(document);
    }
}
