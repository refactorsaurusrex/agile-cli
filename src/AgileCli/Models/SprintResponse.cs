using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace AgileCli.Models
{
    public class SprintResponse
    {
        [JsonProperty("sprints")]
        public List<Sprint> AllSprints { get; set; }

        public IReadOnlyCollection<Sprint> ClosedSprints => AllSprints
            .Where(x => x.State == "CLOSED")
            .OrderByDescending(x => x.Id)
            .ToList()
            .AsReadOnly();
    }
}