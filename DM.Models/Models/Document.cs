using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DM.Models.Models
{
    public class Document
    {
        [Key]
        public Guid DocumentId { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string StoredFileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;

        [Required]
        public long FileSize { get; set; }

        [Required]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    }
}
