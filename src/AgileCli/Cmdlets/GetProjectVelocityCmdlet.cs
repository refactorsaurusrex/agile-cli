﻿using System.Management.Automation;
using AgileCli.Services;
using JetBrains.Annotations;

namespace AgileCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "ProjectVelocity")]
    public class GetProjectVelocityCmdlet : JiraCmdletBase
    {
        protected override void Run()
        {
            var client = new JiraClient(JiraHostName, JiraAccessToken) { DisableCache = NoCache };
            var engine = client.CreateSprintReportEngine(BoardName, SprintCount).Result;
            var avgVelocity = engine.GetVelocityAverages();
            WriteObject(avgVelocity);
        }
    }
}