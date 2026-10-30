using DM.Business.IService;
using DM.Business.Service;
using DM.DataAccess.Migrations;
using DM.Models.Models;
using DocumentManagement_Molina_AevinEarl.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using Xunit;

namespace DM.Tests.Controllers
{
    public class DocumentControllerTest
    {
        private readonly Mock<IDocumentService> _documentService;
        private readonly DocumentController _controller;

        public DocumentControllerTest()
        {
            _documentService = new Mock<IDocumentService>();

            _controller = new DocumentController(_documentService.Object);
        }



        //GetOne
        [Fact]
        public async Task GetOneDocumnet_Returns200k()
        {
            var documentId = Guid.NewGuid();

            var document = new Document
            {
                DocumentId = documentId
            };

            _documentService
           .Setup(x => x.GetOneDocumentAsync(documentId.ToString()))
           .ReturnsAsync(document);


            var result = await _controller.GetOneDocument(
                documentId.ToString()
            );

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(200, okResult.StatusCode);
            Assert.Same(document, okResult.Value);
        }

        [Fact]
        public async Task GetOneDocument_NonexistentId_Returns404NotFound()
        {
            var documentId = Guid.NewGuid().ToString();

            _documentService
                .Setup(x => x.GetOneDocumentAsync(documentId))
                .ReturnsAsync((Document?)null);


            var result = await _controller.GetOneDocument(documentId);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal(
                "No document found with the same Id",
                notFoundResult.Value
            );
        }

        [Fact]
        public async Task GetOneDocument_EmptyId_Returns400BadRequest()
        {

            string documentId = "";


            var result = await _controller.GetOneDocument(documentId);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal(
                "Id cannot be null or empty",
                badRequestResult.Value
            );


            _documentService.Verify(
                x => x.GetOneDocumentAsync(It.IsAny<string>()),
                Times.Never
            );
        }

        //Add Document
        private DocumentRequest CreateValidRequest()
        {
            var fileContent = new MemoryStream(
                System.Text.Encoding.UTF8.GetBytes("Sample document content")
            );

            var file = new FormFile(
                fileContent,
                0,
                fileContent.Length,
                "File",
                "sample.pdf"
            )
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };

            return new DocumentRequest
            {
                File = file
            };
        }

        [Fact]
        public async Task AddDocument_ValidRequest_Returns201Created()
        {

            var request = CreateValidRequest();

            var expectedDocument = new Document
            {
                DocumentId = Guid.NewGuid(),
                OriginalFileName = "sample.pdf",
                StoredFileName = "sample.pdf",
                ContentType = "application/pdf",
                FileSize = request.File.Length,
                UploadedAt = DateTime.UtcNow
            };

            _documentService
                .Setup(s => s.ValidateFile(request));

            _documentService
                .Setup(s => s.CreateDocument(request))
                .ReturnsAsync(expectedDocument);


            var result = await _controller.AddDocument(request);

            var response = Assert.IsType<ObjectResult>(result);

            Assert.Equal(201, response.StatusCode);
            Assert.Equal(expectedDocument, response.Value);

            _documentService.Verify(
                s => s.ValidateFile(request),
                Times.Once
            );

            _documentService.Verify(
                s => s.CreateDocument(request),
                Times.Once
            );
        }


        [Fact]
        public async Task AddDocument_InvalidModelState_Returns400BadRequest()
        {

            var request = CreateValidRequest();

            _controller.ModelState.AddModelError(
                "File",
                "File is required."
            );


            var result = await _controller.AddDocument(request);


            Assert.NotNull(result);

            var response = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(400, response.StatusCode);

            _documentService.Verify(
                s => s.ValidateFile(It.IsAny<DocumentRequest>()),
                Times.Never
            );

            _documentService.Verify(
                s => s.CreateDocument(It.IsAny<DocumentRequest>()),
                Times.Never
            );
        }

        [Fact]
        public async Task AddDocument_InvalidFile_Returns400BadRequest()
        {

            var request = CreateValidRequest();

            _documentService
                .Setup(s => s.ValidateFile(request))
                .Throws(new ArgumentException("Unsupported file extension"));

            var result = await _controller.AddDocument(request);


            var response = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(400, response.StatusCode);

            _documentService.Verify(
                s => s.CreateDocument(It.IsAny<DocumentRequest>()),
                Times.Never
            );
        }


        [Fact]
        public async Task AddDocument_UnsupportedContentType_Returns415()
        {

            var request = CreateValidRequest();

            _documentService
                .Setup(s => s.ValidateFile(request))
                .Throws(new UnsupportedContentTypeException("application/xml"));

            var result = await _controller.AddDocument(request);


            var response = Assert.IsType<ObjectResult>(result);

            Assert.Equal(415, response.StatusCode);
        }


        [Fact]
        public async Task AddDocument_ServiceThrowsException_Returns500()
        {

            var request = CreateValidRequest();

            _documentService
                .Setup(s => s.ValidateFile(request));

            _documentService
                .Setup(s => s.CreateDocument(request))
                .ThrowsAsync(new Exception("Database connection failed"));


            var result = await _controller.AddDocument(request);

            var response = Assert.IsType<ObjectResult>(result);

            Assert.Equal(500, response.StatusCode);
        }


        //Delete Document 

        [Fact]
        public async Task DeleteDocument_ValidId_Returns204NoContent()
        {

            var documentId = Guid.NewGuid();

            var document = new Document
            {
                DocumentId = documentId,
                OriginalFileName = "sample.pdf",
                StoredFileName = "sample.pdf"
            };

            _documentService
                .Setup(s => s.GetOneDocumentAsync(documentId.ToString()))
                .ReturnsAsync(document);

            _documentService
                .Setup(s => s.DeleteDocument(documentId.ToString()))
                .Returns(Task.CompletedTask);


            var result = await _controller.DeleteDocument(documentId.ToString());


            Assert.IsType<NoContentResult>(result);

            _documentService.Verify(
                s => s.GetOneDocumentAsync(documentId.ToString()),
                Times.Once
            );

            _documentService.Verify(
                s => s.DeleteDocument(documentId.ToString()),
                Times.Once
            );
        }


        [Fact]
        public async Task DeleteDocument_EmptyId_Returns400BadRequest()
        {

            var documentId = "";


            var result = await _controller.DeleteDocument(documentId);


            var response = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(400, response.StatusCode);

            _documentService.Verify(
                s => s.GetOneDocumentAsync(It.IsAny<string>()),
                Times.Never
            );
        }


        [Fact]
        public async Task DeleteDocument_DocumentNotFound_Returns404NotFound()
        {
            var documentId = Guid.NewGuid().ToString();

            _documentService
                .Setup(s => s.GetOneDocumentAsync(documentId))
                .ReturnsAsync((Document?)null);


            var result = await _controller.DeleteDocument(documentId);

            var response = Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal(404, response.StatusCode);

            _documentService.Verify(
                s => s.DeleteDocument(It.IsAny<string>()),
                Times.Never
            );
        }


        [Fact]
        public async Task DeleteDocument_ServiceThrowsException_Returns500()
        {

            var documentId = Guid.NewGuid().ToString();

            _documentService
                .Setup(s => s.GetOneDocumentAsync(documentId))
                .ThrowsAsync(new Exception("Database error"));


            var result = await _controller.DeleteDocument(documentId);

            var response = Assert.IsType<ObjectResult>(result);

            Assert.Equal(500, response.StatusCode);
        }



        //Download Document
        private Document CreateTestDocument()
        {
            return new Document
            {
                DocumentId = Guid.NewGuid(),
                OriginalFileName = "sample.pdf",
                StoredFileName = "unique-sample.pdf",
                ContentType = "application/pdf",
                FileSize = 1024,
                UploadedAt = DateTime.UtcNow
            };
        }

        [Fact]
        public async Task DownloadDocument_ExistingDocument_ReturnsFile()
        {
       
            var document = CreateTestDocument();

            var fileContent = new byte[] { 1, 2, 3, 4, 5 };
            var fileStream = new MemoryStream(fileContent);

            _documentService
                .Setup(x => x.GetOneDocumentAsync(document.DocumentId.ToString()))
                .ReturnsAsync(document);

            _documentService
                .Setup(x => x.DownloadAsync(document.StoredFileName))
                .ReturnsAsync(fileStream);

       
            var result = await _controller.DownloadDocument(
                document.DocumentId.ToString()
            );


            var fileResult = Assert.IsType<FileStreamResult>(result);

            Assert.Equal(document.ContentType, fileResult.ContentType);
            Assert.Equal(document.OriginalFileName, fileResult.FileDownloadName);

            var returnedStream = Assert.IsType<MemoryStream>(
                fileResult.FileStream
            );

            Assert.Equal(fileContent, returnedStream.ToArray());

            _documentService.Verify(
                x => x.DownloadAsync(document.StoredFileName),
                Times.Once
            );
        }


        [Fact]
        public async Task DownloadDocument_NonexistentDocument_ReturnsNotFound()
        {
  
            var documentId = Guid.NewGuid().ToString();

            _documentService
                .Setup(x => x.GetOneDocumentAsync(documentId))
                .ReturnsAsync((Document?)null);

       
            var result = await _controller.DownloadDocument(documentId);

      
            Assert.IsType<NotFoundObjectResult>(result);

            _documentService.Verify(
                x => x.DownloadAsync(It.IsAny<string>()),
                Times.Never
            );
        }


        [Fact]
        public async Task DownloadDocument_EmptyId_ReturnsBadRequest()
        {
            var documentId = "";

    
            var result = await _controller.DownloadDocument(documentId);

 
            Assert.IsType<BadRequestObjectResult>(result);

            _documentService.Verify(
                x => x.GetOneDocumentAsync(It.IsAny<string>()),
                Times.Never
            );
        }

  
        [Fact]
        public async Task DownloadDocument_FileDoesNotExist_ReturnsNotFound()
        {
 
            var document = CreateTestDocument();

            _documentService
                .Setup(x => x.GetOneDocumentAsync(document.DocumentId.ToString()))
                .ReturnsAsync(document);

            _documentService
                .Setup(x => x.DownloadAsync(document.StoredFileName))
                .ThrowsAsync(new FileNotFoundException());


            var result = await _controller.DownloadDocument(
                document.DocumentId.ToString()
            );

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DownloadDocument_InvalidPath_ReturnsForbidden()
        {
            var document = CreateTestDocument();

            _documentService
                .Setup(x => x.GetOneDocumentAsync(document.DocumentId.ToString()))
                .ReturnsAsync(document);

            _documentService
                .Setup(x => x.DownloadAsync(document.StoredFileName))
                .ThrowsAsync(new UnauthorizedAccessException());


            var result = await _controller.DownloadDocument(
                document.DocumentId.ToString()
            );


            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(403, objectResult.StatusCode);
        }

        [Fact]
        public async Task DownloadDocument_UnexpectedException_ReturnsInternalServerError()
        {
            var document = CreateTestDocument();

            _documentService
                .Setup(x => x.GetOneDocumentAsync(document.DocumentId.ToString()))
                .ReturnsAsync(document);

            _documentService
                .Setup(x => x.DownloadAsync(document.StoredFileName))
                .ThrowsAsync(new Exception("Unexpected storage error"));

            var result = await _controller.DownloadDocument(
                document.DocumentId.ToString()
            );

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(500, objectResult.StatusCode);
        }

        //Get All

        [Fact]
        public async Task GetAllDocuments_ExistingDocuments_ReturnsOk()
        {
            var documents = new List<Document>
            {
                new Document
                {
                    DocumentId = Guid.NewGuid(),
                    OriginalFileName = "document1.pdf",
                    StoredFileName = "unique-document1.pdf",
                    ContentType = "application/pdf",
                    FileSize = 1024,
                    UploadedAt = DateTime.UtcNow
                },
                new Document
                {
                    DocumentId = Guid.NewGuid(),
                    OriginalFileName = "document2.docx",
                    StoredFileName = "unique-document2.docx",
                    ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    FileSize = 2048,
                    UploadedAt = DateTime.UtcNow
                }
            };

            _documentService
                .Setup(x => x.GetAllDocumentsAsync())
                .ReturnsAsync(documents.AsEnumerable());

         
            var result = await _controller.GetAllDocuments();

            
            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnedDocuments = Assert.IsAssignableFrom<IEnumerable<Document>>(
                okResult.Value
            );

            Assert.Equal(2, returnedDocuments.Count());

            _documentService.Verify(
                x => x.GetAllDocumentsAsync(),
                Times.Once
            );
        }


        [Fact]
        public async Task GetAllDocuments_NoDocuments_ReturnsNoContent()
        {
            var documents = new List<Document>();

            _documentService
                .Setup(x => x.GetAllDocumentsAsync())
                .ReturnsAsync(documents.AsEnumerable());

            var result = await _controller.GetAllDocuments();

            Assert.IsType<NoContentResult>(result);

            _documentService.Verify(
                x => x.GetAllDocumentsAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task GetAllDocuments_ServiceThrowsException_ReturnsInternalServerError()
        {
         
            _documentService
                .Setup(x => x.GetAllDocumentsAsync())
                .ThrowsAsync(new Exception("Database connection failed"));

      
            var result = await _controller.GetAllDocuments();

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(
                StatusCodes.Status500InternalServerError,
                objectResult.StatusCode
            );

            _documentService.Verify(
                x => x.GetAllDocumentsAsync(),
                Times.Once
            );
        }



    }
}
