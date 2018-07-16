using System;
using System.ComponentModel.DataAnnotations;

namespace Voidwell.Internal.Models
{
    public class BlogPostModel
    {
        public string Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTimeOffset PublishDate { get; set; }
        [Required]
        public string Content { get; set; }
    }
}
