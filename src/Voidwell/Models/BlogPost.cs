using System;

namespace Voidwell.Models
{
    public class BlogPost
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime PublishDate { get; set; }
        public string Content { get; set; }
    }
}
