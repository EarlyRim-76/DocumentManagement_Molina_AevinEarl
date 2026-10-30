using DM.Business.IService;
using DM.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DocumentManagement_Molina_AevinEarl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {

        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }




        [HttpGet]
        [ActionName("document")]
        public async Task<IActionResult> GetAllDocuments()
        {
            try
            {

                var documents = await _documentService.GetAllDocumentsAsync();

                if (!documents.Any())
                {
                    return NoContent();
                }

                return Ok(documents);

            }catch(Exception e)
            {
                return Problem(detail : "An unexpected error occurred while fetching all documents", statusCode: StatusCodes.Status500InternalServerError, title: "Get all error");
            }
        }


        [HttpGet("{documentId}")]
        [ActionName("document")]
        public async Task<IActionResult> GetOneDocument([FromRoute] string documentId)
        {
            try
            {

                if(string.IsNullOrEmpty(documentId))
                {
                    return BadRequest("Id cannot be null or empty");
                }

                var document = await _documentService.GetOneDocumentAsync(documentId);

                if (document == null)
                {
                    return NotFound("No document found with the same Id");
                }

                return Ok(document);

            }
            catch (Exception e)
            {
                return Problem(detail: "An unexpected error occured while fetching document", statusCode: StatusCodes.Status500InternalServerError, title: "Get one error");
            }
        }


        [HttpPost]
        [ActionName("document")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddDocument([FromForm] DocumentRequest request)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _documentService.ValidateFile(request);

                var document = await _documentService.CreateDocument(request);

                return StatusCode(StatusCodes.Status201Created, document);

            }
            catch(ArgumentException ex)
            {
                return BadRequest(new {message = ex.Message});
            }
            catch(UnsupportedContentTypeException e)
            {
                return StatusCode(StatusCodes.Status415UnsupportedMediaType, new { message = e.Message });
            }
            catch (Exception e)
            {
                return Problem(detail: "An unexpected error occurred while creating document", statusCode: StatusCodes.Status500InternalServerError, title: "Add document error");
            }
        
        }

        [HttpGet("{documentId}/download")]
        public async Task<IActionResult> DownloadDocument(string documentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(documentId))
                {
                    return BadRequest("Id cannot be null or empty");
                }

                var document = await _documentService.GetOneDocumentAsync(documentId);

                if (document == null)
                {
                    return NotFound("No document found with the same Id");
                }

                var stream = await _documentService.DownloadAsync(document.StoredFileName);

                if (stream == null)
                {
                    return NotFound("Physical document file not found");
                }

                return File(
                    stream,
                    document.ContentType,
                    document.OriginalFileName
                );
            }
            catch (FileNotFoundException)
            {
                return NotFound("Physical document file not found");
            }
            catch (UnauthorizedAccessException)
            {
                return Problem(
                    detail: "You are not authorized to access this file.",
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Access denied"
                );
            }
            catch (Exception)
            {
                return Problem(
                    detail: "An unexpected error occurred while downloading document.",
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Download error"
                );
            }
        }

        [HttpDelete("{documentId}")]
        public async Task<IActionResult> DeleteDocument(string documentId)
        {
            try
            {

                if (string.IsNullOrEmpty(documentId))
                {
                    return BadRequest("Id cannot be null or empty");
                }

                var document = await _documentService.GetOneDocumentAsync(documentId);

                if (document == null)
                {
                    return NotFound("No document found with the same Id");
                }

                await _documentService.DeleteDocument(document.DocumentId.ToString());
                return NoContent();

            }
            catch (Exception ex)
            {
                return Problem(detail: "An unexpected error occurred while deleted the document", statusCode: StatusCodes.Status500InternalServerError, title: "Delete error");
            }
        }

    }
}
