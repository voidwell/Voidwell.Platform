using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Voidwell.Internal.Data.Models
{
    [Table("CustomEventScores")]
    public class DbCustomEventScores
    {
        [Required]
        public int EventId { get; set; }
        [Required]
        public string TeamId { get; set; }
        [Required]
        public string Score { get; set; }
    }
}