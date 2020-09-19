using System.Management.Automation;
using AgileCli.Services;
using JetBrains.Annotations;

namespace AgileCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "UserPointAverages")]
    public class GetUserPointAveragesCmdlet : JiraCmdletBase
    {
        [Parameter]
        public SwitchParameter Raw { get; set; }

        protected override void Run()
        {
            var client = new JiraClient(JiraHostName, JiraAccessToken) { DisableCache = NoCache };
            var engine = client.CreateSprintReportEngine(BoardName, SprintCount).Result;
            var report = engine.GetUserAverages(Raw);
            WriteObject(report, true);
        }
    }
}