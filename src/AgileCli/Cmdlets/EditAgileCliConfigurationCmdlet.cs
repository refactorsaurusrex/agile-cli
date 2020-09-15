using System.Management.Automation;
using AgileCli.Services;
using JetBrains.Annotations;

namespace AgileCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsData.Edit, "AgileCliConfiguration")]
    public class EditAgileCliConfigurationCmdlet : CmdletBase
    {
        protected override void Run()
        {
            var config = AgileCliConfigurationManager.Load();
            var process = SystemProcessFactory.Create();
            config.Open(process);
        }
    }
}