using System;
using System.Management.Automation;
using AgileCli.Infrastructure;
using AgileCli.Services;
using JetBrains.Annotations;

namespace AgileCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsData.Save, "JiraToken")]
    public class SaveJiraTokenCmdlet : CmdletBase
    {
        protected override void Run()
        {
            const string caption = "Enter your Jira username and access token below. Note that you MUST use an access token, not your password. " +
                                   "Normal passwords will not work.";
            var creds = Host.UI.PromptForCredential(caption, "", "", "");
            var networkCreds = creds.GetNetworkCredential();
            var combined = $"{networkCreds.UserName}:{networkCreds.Password}";
            var bytes = System.Text.Encoding.UTF8.GetBytes(combined);
            var encoded = Convert.ToBase64String(bytes);

            var tokenManager = TokenManagerFactory.Create(Secrets.JiraToken);
            tokenManager.Store(encoded);
        }
    }
}