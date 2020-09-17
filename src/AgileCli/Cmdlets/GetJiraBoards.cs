using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using AgileCli.Services;
using JetBrains.Annotations;

namespace AgileCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "JiraBoards")]
    [OutputType(typeof(IEnumerable<string>))]
    public class GetJiraBoards : JiraCmdletBase
    {
        // Override to hide. Properties not used here.
        public override string BoardName { get; set; }
        public override int SprintCount { get; set; }

        protected override void Run()
        {
            var client = new JiraClient(JiraHostName, JiraAccessToken) { DisableCache = NoCache };
            var boards = client.GetBoards().Result;
            var filtered = boards.OrderBy(x => x.Name).Select(x => new { x.Name });
            WriteObject(filtered, true);
        }
    }
}