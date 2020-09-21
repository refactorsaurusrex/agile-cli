using System.Linq;
using System.Management.Automation;
using JetBrains.Annotations;

namespace AgileCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Format, "UserPointAverages")]
    public class FormatUserPointAveragesCmdlet : CmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public dynamic[] Data { get; set; }

        protected override void Run()
        {
            var results = Data.Select(x => new
            {
                x.Assignee,
                AverageCommitted = x.AverageCommitted.ToString("N1"),
                AverageCompleted = x.AverageCompleted.ToString("N1"),
                AverageRollover = x.AverageRollover.ToString("N1"),
                RolloverPercent = x.RolloverPercent.ToString("p0")
            });

            WriteObject(results);
        }
    }
}