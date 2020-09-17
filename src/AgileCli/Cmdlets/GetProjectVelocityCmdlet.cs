using System.Management.Automation;
using AgileCli.Services;
using AsyncProgressReporter;
using JetBrains.Annotations;

namespace AgileCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "ProjectVelocity")]
    public class GetProjectVelocityCmdlet : JiraCmdletBase
    {
        protected override void Run()
        {
            var client = new JiraClient(JiraHostName, JiraAccessToken) { DisableCache = NoCache };
            var reporter = new ProgressReporter();
            var task = client.CreateSprintReportEngine(BoardName, SprintCount, reporter);
            ShowProgressWait(reporter, "Getting sprint history...", statusDescriptionMap: StatusMap);
            var engine = task.Result;
            var avgVelocity = engine.GetVelocityAverages();
            WriteObject(avgVelocity);
        }
    }
}