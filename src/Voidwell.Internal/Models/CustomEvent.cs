using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Voidwell.Internal.Models
{
    public class CustomEvent
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ServerId { get; set; }
        public string MapId { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsPrivate { get; set; }
        public string GameId { get; set; }
        public IEnumerable<CustomEventTeam> Teams { get; set; }

        public JToken Log { get; set; }
        public JToken Score { get; set; }
    }
}
