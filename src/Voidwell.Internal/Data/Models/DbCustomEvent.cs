using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Voidwell.Internal.Data.Models
{
    [Table("CustomEvent")]
    public class DbCustomEvent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string ServerId { get; set; }
        public string MapId { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsPrivate { get; set; }
        public string GameId { get; set; }

        [ForeignKey("Id")]
        public IEnumerable<DbCustomEventTeam> Teams { get; set; }
    }
}
