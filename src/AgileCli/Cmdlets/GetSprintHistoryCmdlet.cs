using System.Management.Automation;
using AgileCli.Services;
using JetBrains.Annotations;

namespace AgileCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "SprintHistory")]
    public class GetSprintHistoryCmdlet : JiraCmdletBase
    {
        private PSReportEngine _engine;

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            _engine = CreateReportingEngine();
        }

        protected override void Run()
        {
            var velocityReport = _engine.GetVelocityReport();
            WriteObject(velocityReport, true);
        }
    }
}
