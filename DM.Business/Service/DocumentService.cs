using DM.Business.IService;
using DM.DataAccess.Context;
using DM.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace DM.Business.Service
{
    public class DocumentService : IDocumentService
    {
        private readonly ApplicationDBContext _context;
        private readonly string _storagePath;
        private readonly string _maxSize;

        private readonly string[] _acceptedFileType;


        public DocumentService(ApplicationDBContext context, IConfiguration config )
        {
            _context = context;
            _storagePath = config["FileStorage:RootPath"];
            _maxSize = config["FileStorage:MaxFileSize"];
            _acceptedFileType = config.GetSection("FileStorage:AllowedExtensions")
                  .GetChildren()
                  .Select(x => x.Value)
                  .ToArray();

            Directory.CreateDirectory(_storagePath);
        }


        public async Task<Document> CreateDocument(DocumentRequest request)
        {




            Guid uniqueId = Guid.NewGuid();

            Document document = new()
            {
                DocumentId = uniqueId,
                ContentType = request.File.ContentType,
                FileSize = request.File.Length,
                OriginalFileName = request.File.FileName,
                StoredFileName = $"{uniqueId}_{request.File.FileName}",
                UploadedAt = DateTime.UtcNow
            };

            var filePath = Path.Combine(_storagePath, document.StoredFileName);

            await using var stream = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                useAsync:true
                );
            {
                await request.File.CopyToAsync(stream);
            }
           


            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
            return document; 


        }

    
        public async Task<IEnumerable<Document>> GetAllDocumentsAsync()
        {
            var documents = await _context.Documents.ToListAsync();

            return documents;
        }

        public async Task<Document?> GetOneDocumentAsync(string documentId)
        {
            var document = await _context.Documents.FirstOrDefaultAsync(x => x.DocumentId.ToString() == documentId);

            return document;
            
        }

        public async Task<Stream?> DownloadAsync(string storedFileName)
        {
            if (string.IsNullOrWhiteSpace(storedFileName))
            {
                throw new ArgumentException("Stored file name cannot be empty.");
            }

            var storagePath = FixStoragePath(_storagePath);

            var fullPath = Path.GetFullPath(
                Path.Combine(storagePath, storedFileName)
            );

            if (!fullPath.StartsWith(
                storagePath + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Invalid file path.");
            }

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("File not found.");
            }

            Stream stream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                useAsync: true
            );

            return stream;
        }

        private string FixStoragePath(string storagePath)
        {
            return Path.GetFullPath(storagePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }


        public async Task DeleteDocument(string documentId)
        {

            Guid id = Guid.Parse(documentId);
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                throw new KeyNotFoundException($"Document {documentId} ket not found");
            }

            var trimPath = FixStoragePath(_storagePath);

            var fullPath = Path.GetFullPath(Path.Combine(trimPath, document.StoredFileName));

            if (!fullPath.StartsWith(trimPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Invalid file Path");
            }

            File.Delete(fullPath);

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();


        }

        public void ValidateFile(DocumentRequest request)
        {
           if(request.File == null || request.File.Length == 0)
            {
                throw new ArgumentException("Please select a file");
            }

            var maxSize = long.Parse(_maxSize);

            if(request.File.Length > maxSize)
            {
                throw new ArgumentException("File size must not exceed 10MB.");
            }
            

            var extention = Path.GetExtension(request.File.FileName);
    

            if (!_acceptedFileType.Contains(extention, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Unsupported file extension");
            }
        }
    }
}
