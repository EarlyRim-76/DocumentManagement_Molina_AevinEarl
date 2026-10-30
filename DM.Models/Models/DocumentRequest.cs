using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DM.Models.Models
{
    public class DocumentRequest
    {
        [Required]
        public IFormFile? File { get; set; }

    }
}
