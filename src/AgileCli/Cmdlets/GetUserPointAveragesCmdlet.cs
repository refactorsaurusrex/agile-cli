using System.Management.Automation;
using AgileCli.Services;
using JetBrains.Annotations;

namespace AgileCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "UserPointAverages")]
    public class GetUserPointAveragesCmdlet : JiraCmdletBase
    {
        private PSReportEngine _engine;

        [Parameter]
        public SwitchParameter Raw { get; set; }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            _engine = CreateReportingEngine();
        }

        protected override void Run()
        {
            var report = _engine.GetUserAverages(Raw);
            WriteObject(report, true);
        }
    }
}