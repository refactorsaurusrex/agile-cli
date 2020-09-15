using System.Collections.Generic;
using Newtonsoft.Json;

namespace AgileCli.Models
{
    public class BoardResponse
    {
        [JsonProperty("views")]
        public List<Board> Boards { get; set; }
    }
}