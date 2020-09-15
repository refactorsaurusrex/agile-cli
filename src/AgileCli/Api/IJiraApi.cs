using System.Threading.Tasks;
using AgileCli.Models;
using Newtonsoft.Json.Linq;
using Refit;
// ReSharper disable StringLiteralTypo

namespace AgileCli.Api
{
    [Headers("Authorization: Basic")]
    public interface IJiraApi
    {
        [Get("/rest/greenhopper/1.0/rapidview")]
        Task<BoardResponse> GetBoards();        

        [Get("/rest/greenhopper/1.0/sprintquery/{rapidViewId}?includeFutureSprints=false")]
        Task<SprintResponse> GetSprints(int rapidViewId);

        [Get("/rest/greenhopper/1.0/rapid/charts/scopechangeburndownchart.json?rapidViewId={rapidViewId}&sprintId={sprintId}")]
        Task<JObject> GetSprintDetails(int rapidViewId, int sprintId);
    }
}
