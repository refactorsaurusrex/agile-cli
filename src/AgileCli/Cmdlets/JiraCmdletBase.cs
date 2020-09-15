﻿using System.Management.Automation;
using AgileCli.Infrastructure;
using AgileCli.Services;

namespace AgileCli.Cmdlets
{
    public abstract class JiraCmdletBase : CmdletBase
    {
        [Parameter]
        public SwitchParameter NoCache { get; set; }

        [Parameter]
        public virtual string BoardName { get; set; }

        [Parameter]
        public virtual int SprintCount { get; set; } = 10;

        protected string JiraAccessToken { get; private set; }

        protected string JiraHostName { get; private set; }

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
        }
    }
}