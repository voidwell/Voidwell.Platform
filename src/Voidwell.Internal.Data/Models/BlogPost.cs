using System;
using System.ComponentModel.DataAnnotations;

namespace Voidwell.Internal.Data.Models
{
    public class BlogPost
    {
        public string Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public Guid AuthorId { get; set; }
        [Required]
        public DateTime PublishDate { get; set; }
        [Required]
        public string Content { get; set; }
    }
}
