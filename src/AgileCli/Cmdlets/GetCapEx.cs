using System.Linq;
using System.Management.Automation;
using JetBrains.Annotations;

namespace AgileCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "CapEx")]
    public class GetCapEx: JiraCmdletBase
    {
        [Parameter]
        public string JQL { get; set; }

        protected override void Run()
        {
            var client = GetJiraClient();
            var issueKeyResponse = client.GetIssueKeys(JQL).Result;
            var capExResults = client.GetChangeLogs(issueKeyResponse).Result;
            WriteObject(capExResults, true);
        }
    }
}