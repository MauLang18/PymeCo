// POS.Infrastructure/FileStorage/IFileStorageLocal.cs
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace POS.Infrastructure.FileStorage
{
    public interface IFileStorageLocal
    {
        /// <summary>Guarda el archivo y retorna la URL pública (ej: /uploads/products/abc.jpg)</summary>
        Task<string> SaveAsync(IFormFile file, string subfolder = "products");
        /// <summary>Elimina un archivo previo dada la URL relativa (opcional)</summary>
        Task DeleteAsync(string relativeUrl);
    }
}
