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
            var process = SystemProcessFactory.Create();

            try
            {
                var config = AgileCliConfigurationManager.Load();
                config.Open(process);
            }
            catch (AgileCliConfigurationException e)
            {
                process.Start(e.ConfigurationFilePath);
                WriteError(new ErrorRecord(e, "", ErrorCategory.InvalidOperation, null));
            }
        }
    }
}