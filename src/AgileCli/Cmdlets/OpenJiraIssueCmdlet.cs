using System.Management.Automation;
using AgileCli.Services;
using JetBrains.Annotations;

namespace AgileCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Open, "JiraIssue")]
    public class OpenJiraIssueCmdlet : CmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public string Key { get; set; }

        protected override void Run()
        {
            var hostname = AgileCliConfigurationManager.Load().JiraHostName;
            SystemProcessFactory.Create().Start($"https://{hostname}/browse/{Key}");
        }
    }
}