using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Voidwell.Internal.Data.Models
{
    [Table("CustomEventTeam")]
    public class DbCustomEventTeam
    {
        [Required]
        public string EventId { get; set; }
        [Required]
        public string TeamId { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
