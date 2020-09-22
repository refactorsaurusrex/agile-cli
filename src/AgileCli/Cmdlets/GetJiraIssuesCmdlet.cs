using System.Management.Automation;
using AgileCli.Infrastructure;
using AgileCli.Services;
using JetBrains.Annotations;

namespace AgileCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "JiraIssues")]
    public class GetJiraIssuesCmdlet : JiraCmdletBase
    {
        private PSReportEngine _engine;

        [Parameter]
        [ValidateSet("Committed", "Completed", "Rollover", "Unplanned")]
        public string Type { get; set; }

        [Parameter]
        public string Assignee { get; set; }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            _engine = CreateReportingEngine();
        }

        protected override void Run()
        {
            var type = string.IsNullOrWhiteSpace(Type) ? IssueType.Committed : Type.ToEnum<IssueType>();
            var issues = _engine.GetIssues(type, Assignee);
            WriteObject(issues, true);
        }
    }
}