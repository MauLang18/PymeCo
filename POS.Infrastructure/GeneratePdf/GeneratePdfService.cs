using System.Text;
using System.Text.RegularExpressions;
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

    public byte[] GeneratePdf<T>(string htmlTemplate, T data)
    {
        if (string.IsNullOrWhiteSpace(htmlTemplate))
            throw new ArgumentException("HTML template cannot be null or empty.");

        var populatedHtml = PopulateTemplate(htmlTemplate, data);

        return GeneratePdfFromHtml(populatedHtml);
    }

    // =========================
    // Template engine (simple)
    // =========================
    private string PopulateTemplate<T>(string templateContent, T data)
    {
        var populatedHtml = templateContent;

        // Replace simple properties {{PropertyName}}
        foreach (var property in typeof(T).GetProperties())
        {
            var key = $"{{{{{property.Name}}}}}";
            var value = property.GetValue(data)?.ToString() ?? string.Empty;
            populatedHtml = populatedHtml.Replace(key, value);
        }

        // Handle collections {{#each Collection}} ... {{/each}}
        var matches = Regex.Matches(
            populatedHtml,
            @"{{#each\s+(?<Collection>\w+)}}(?<Content>.*?){{\/each}}",
            RegexOptions.Singleline
        );

        foreach (Match match in matches)
        {
            var collectionName = match.Groups["Collection"].Value;
            var itemTemplate = match.Groups["Content"].Value;

            var collectionProperty = typeof(T).GetProperty(collectionName);
            var collection = collectionProperty?.GetValue(data) as System.Collections.IEnumerable;

            if (collection == null)
            {
                populatedHtml = populatedHtml.Replace(match.Value, string.Empty);
                continue;
            }

            var collectionHtml = new StringBuilder();

            foreach (var item in collection)
            {
                var itemContent = itemTemplate;

                foreach (var itemProperty in item.GetType().GetProperties())
                {
                    var itemKey = $"{{{{this.{itemProperty.Name}}}}}";
                    var itemValue = itemProperty.GetValue(item)?.ToString() ?? string.Empty;
                    itemContent = itemContent.Replace(itemKey, itemValue);
                }

                collectionHtml.Append(itemContent);
            }

            populatedHtml = populatedHtml.Replace(match.Value, collectionHtml.ToString());
        }

        return populatedHtml;
    }

    // =========================
    // PDF generation
    // =========================
    private byte[] GeneratePdfFromHtml(string htmlContent)
    {
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
