﻿namespace AgileCli.Services
{
    internal interface IAgileCliConfiguration
    {
        string JiraHostName { get; set; }
        string DefaultBoardName { get; set; }
        int DefaultSprintCount { get; set; }
        double GetSprintTargetPercent();
        void Save();
        void Open(ISystemProcess process);
    }
}