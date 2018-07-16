using System.ComponentModel.DataAnnotations;

namespace Voidwell.Internal.Data.Models
{
    public class CustomEventTeam
    {
        [Required]
        public int CustomEventId { get; set; }
        [Required]
        public string TeamId { get; set; }
        [Required]
        public string Name { get; set; }

        public CustomEvent CustomEvent { get; set; }
    }
}
