using System.Management.Automation;
using AgileCli.Services;
using JetBrains.Annotations;

namespace AgileCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "ProjectVelocity")]
    public class GetProjectVelocityCmdlet : JiraCmdletBase
    {
        private PSReportEngine _engine;

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            _engine = CreateReportingEngine();
        }

        protected override void Run()
        {
            var avgVelocity = _engine.GetVelocityAverages();
            WriteObject(avgVelocity);
        }
    }
}