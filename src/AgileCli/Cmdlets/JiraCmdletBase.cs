using System.Management.Automation;
using AgileCli.Infrastructure;
using AgileCli.Services;
using AsyncProgressReporter;

namespace AgileCli.Cmdlets
{
    public abstract class JiraCmdletBase : CmdletBase
    {
        [Parameter]
        public SwitchParameter NoCache { get; set; }

        [Parameter]
        public virtual string BoardName { get; set; }

        [Parameter]
        public virtual int SprintCount { get; set; }

        protected string JiraAccessToken { get; private set; }

        protected string JiraHostName { get; private set; }

        protected string StatusMap(ProgressInfo info) => $"Completed {info.CompletedItems} of {info.TotalItems} sprints";

        protected override void BeginProcessing()
        {
            const string missingToken = "A Jira access token does not exist on this machine. Please use the 'Save-JiraToken' cmdlet to securely store your Jira access token.";
            var token = TokenManagerFactory.Create(Secrets.JiraToken).Retrieve();
            if (string.IsNullOrWhiteSpace(token))
                throw new PSInvalidOperationException(missingToken);
            JiraAccessToken = token;

            const string missingHostName = "A Jira host name has not been set. Please use the 'Edit-AgileCliConfiguration' cmdlet to enter a hostname.";
            var config = AgileCliConfigurationManager.Load();
            if (string.IsNullOrWhiteSpace(config.JiraHostName))
                throw new PSInvalidOperationException(missingHostName);
            JiraHostName = config.JiraHostName;

            const string missingBoard = "A board name was not specified and no default board name has been saved. Please either use the '-BoardName' "+
                                        "parameter, or use the 'Edit-AgileCliConfiguration' cmdlet to save a default board name.";
            if (string.IsNullOrWhiteSpace(BoardName))
            {
                if (string.IsNullOrWhiteSpace(config.DefaultBoardName))
                    throw new PSInvalidOperationException(missingBoard);
                BoardName = config.DefaultBoardName;
            }

            if (SprintCount == 0)
                SprintCount = config.DefaultSprintCount;
        }

        protected PSReportEngine CreateReportingEngine()
        {
            var client = new JiraClient(JiraHostName, JiraAccessToken) { DisableCache = NoCache };
            var task = client.CreateSprintReportEngine(BoardName, SprintCount);
            ShowElapsedTimeProgress(task, "Getting Jira Sprint Data", "Please wait, this should only take a few seconds...");
            HideProgress();
            return task.Result;
        }
    }
}