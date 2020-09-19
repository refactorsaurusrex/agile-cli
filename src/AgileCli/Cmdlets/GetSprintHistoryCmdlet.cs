using System.Management.Automation;
using AgileCli.Services;
using AsyncProgressReporter;
using JetBrains.Annotations;

namespace AgileCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "SprintHistory")]
    public class GetSprintHistoryCmdlet : JiraCmdletBase
    {
        protected override void Run()
        {
            var client = new JiraClient(JiraHostName, JiraAccessToken) { DisableCache = NoCache };
            var engine = client.CreateSprintReportEngine(BoardName, SprintCount).Result;
            var velocityReport = engine.GetVelocityReport();
            WriteObject(velocityReport, true);
        }
    }
}
