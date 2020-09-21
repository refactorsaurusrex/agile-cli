using System.Management.Automation;
using AgileCli.Services;
using JetBrains.Annotations;

namespace AgileCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "PercentOnTargetSprints")]
    public class GetPercentOnTargetSprintsCmdlet : JiraCmdletBase
    {
        private PSReportEngine _engine;

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            _engine = CreateReportingEngine();
        }

        protected override void Run()
        {
            var config = AgileCliConfigurationManager.Load();
            var onTarget = _engine.GetPercentOnTargetSprints(config.GetSprintTargetPercent());
            WriteObject(onTarget);
        }
    }
}