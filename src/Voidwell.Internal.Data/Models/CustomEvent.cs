using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Voidwell.Internal.Data.Models
{
    public class CustomEvent
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string ServerId { get; set; }
        public string MapId { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsPrivate { get; set; }
        public string GameId { get; set; }
        public string ScoreConfiguration { get; set; }

        public IEnumerable<CustomEventTeam> Teams { get; set; }
    }
}
