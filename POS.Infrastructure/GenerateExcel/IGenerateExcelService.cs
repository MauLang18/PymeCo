namespace POS.Infrastructure.GenerateExcel;

public interface IGenerateExcelService
{
    byte[] GenerateExcel<T>(
        IEnumerable<T> data,
        List<(string ColumnName, string PropertyName)> columns
    );
}
