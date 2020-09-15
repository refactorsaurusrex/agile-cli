using System.Management.Automation;
using AgileCli.Services;
using JetBrains.Annotations;

namespace AgileCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "PercentOnTargetSprints")]
    public class GetPercentOnTargetSprintsCmdlet : JiraCmdletBase
    {
        protected override void Run()
        {
            var client = new JiraClient(JiraHostName, JiraAccessToken) { DisableCache = NoCache };
            var engine = client.CreateSprintReportEngine(BoardName, SprintCount).Result;
            var config = AgileCliConfigurationManager.Load();
            var onTarget = engine.GetPercentOnTargetSprints(config.GetSprintTargetPercent());
            WriteObject(onTarget);
        }
    }
}