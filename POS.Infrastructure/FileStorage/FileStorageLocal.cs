using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace POS.Infrastructure.FileStorage
{
    public class FileStorageLocal : IFileStorageLocal
    {
        private readonly string _wwwroot;

        public FileStorageLocal(IWebHostEnvironment env)
        {
            _wwwroot = env.WebRootPath ?? throw new InvalidOperationException("WebRootPath no está configurado.");
        }

        public async Task<string> SaveAsync(IFormFile file, string subfolder = "products")
        {
            if (file == null || file.Length == 0) throw new ArgumentException("Archivo vacío.", nameof(file));

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".bin";

            var name = $"{Guid.NewGuid():N}{ext}";
            var folderAbs = Path.Combine(_wwwroot, "uploads", subfolder);
            Directory.CreateDirectory(folderAbs);

            var absPath = Path.Combine(folderAbs, name);
            using (var fs = new FileStream(absPath, FileMode.Create)) { await file.CopyToAsync(fs); }

            // URL relativa que se guardará en DB
            return $"/uploads/{subfolder}/{name}";
        }

        public Task DeleteAsync(string relativeUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeUrl)) return Task.CompletedTask;
            var rel = relativeUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
            var full = Path.Combine(_wwwroot, rel);
            if (File.Exists(full)) File.Delete(full);
            return Task.CompletedTask;
        }
    }
}
