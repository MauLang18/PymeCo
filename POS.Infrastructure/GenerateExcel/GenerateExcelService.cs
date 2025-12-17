using ClosedXML.Excel;

namespace POS.Infrastructure.GenerateExcel;

public class GenerateExcelService : IGenerateExcelService
{
    public byte[] GenerateExcel<T>(
        IEnumerable<T> data,
        List<(string ColumnName, string PropertyName)> columns
    )
    {
        var columnNames = columns.Select(c => c.ColumnName).ToList();

        using (var memoryStream = new MemoryStream())
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Listado");

                for (int i = 0; i < columnNames.Count; i++)
                {
                    worksheet.Cell(1, i + 1).Value = columnNames[i];
                }

                int rowIndex = 2;
                foreach (var item in data)
                {
                    for (int i = 0; i < columns.Count; i++)
                    {
                        var propertyValue = typeof(T)
                            .GetProperty(columns[i].PropertyName)
                            ?.GetValue(item)
                            ?.ToString();
                        worksheet.Cell(rowIndex, i + 1).Value = propertyValue;
                    }

                    rowIndex++;
                }

                workbook.SaveAs(memoryStream);
            }

            return memoryStream.ToArray();
        }
    }
}
