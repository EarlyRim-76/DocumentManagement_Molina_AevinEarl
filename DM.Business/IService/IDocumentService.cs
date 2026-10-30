using DM.Models.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace DM.Business.IService
{
    public interface IDocumentService
    {
        Task<IEnumerable<Document>> GetAllDocumentsAsync();

        Task<Document?> GetOneDocumentAsync(string documentId);

        Task<Document> CreateDocument(DocumentRequest request);

        Task<Stream?> DownloadAsync(string storedFileName);

        Task DeleteDocument(string documentId);

        void ValidateFile(DocumentRequest request);

    }
}
