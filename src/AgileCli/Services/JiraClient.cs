using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AgileCli.Api;
using AgileCli.Models;
using Newtonsoft.Json.Linq;

namespace AgileCli.Services
{
    public class JiraClient
    {
        private readonly IJiraApi _jira;
        private static readonly CacheItemPolicy CachePolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(60) };

        public JiraClient(string jiraHostname, string accessToken) => _jira = AuthenticatedRestService.For<IJiraApi>(accessToken, $"https://{jiraHostname}");

        public bool DisableCache { get; set; }

        public async Task<IEnumerable<Board>> GetBoards()
        {
            if (!DisableCache && MemoryCache.Default["GetBoards"] is List<Board> boards)
                return boards;

            var boardsResponse = await _jira.GetBoards();
            return boardsResponse.Boards;
        }

        public async Task<List<IssueKey>> GetIssueKeys(string jql)
        {
            //if (!DisableCache && MemoryCache.Default[nameof(GetIssueKeys)] is List<Board> boards)
            //    return boards;

            var keys = new List<IssueKey>();
            var startAt = 0;
            while (true)
            {
                var issueKeys = await _jira.GetIssueKeys(new JqlQuery { Jql = jql, StartAt = startAt });
                keys.AddRange(issueKeys.Issues);

                if (issueKeys.IsLast)
                    break;

                startAt = issueKeys.StartAt + issueKeys.MaxResults;
            }

            return keys;
        }

        public async Task<List<CapExResult>> GetChangeLogs(IEnumerable<IssueKey> keys)
        {
            var result = new List<CapExResult>();

            foreach (var key in keys)
            {
                var response = await _jira.GetIssueChangeLog(key.Key);
                var changes = response.Changes.Where(x => x.Details.Any(y => y.Type == "status")).ToList();

                var timeline = new IssueTimeline(changes);

                var capex = new CapExResult
                {
                    Assignee = key.Fields.Assignee.DisplayName,
                    Key = key.Key,
                    TotalDays = timeline.TotalDaysInProgress(),
                    ResolutionDate = key.Fields.ResolutionDate.ToShortDateString()
                    //StartDate = changes.Select(x => x.Timestamp).Min(),
                    //EndDate = changes.Select(x => x.Timestamp).Max()
                };

                result.Add(capex);

            }

            return result;
        }

        public async Task<PSReportEngine> CreateSprintReportEngine(string boardName, int sprintCount)
        {
            var cacheKey = $"PSReportEngine{boardName}{sprintCount}";
            if (!DisableCache && MemoryCache.Default[cacheKey] is PSReportEngine cachedEngine)
                return cachedEngine;

            var allBoardsResponse = await _jira.GetBoards();
            var targetBoard = allBoardsResponse.Boards.SingleOrDefault(x => x.Name.Equals(boardName, StringComparison.InvariantCultureIgnoreCase)) ??
                              throw new InvalidOperationException($"Unable to locate the board '{boardName}'.");

            var sprintCollection = await _jira.GetSprints(targetBoard.Id);
            var filteredSprints = sprintCollection.ClosedSprints.Take(sprintCount).ToList();

            var sprintRequests = new Queue<(Sprint, Task<JObject>)>();
            foreach (var sprint in filteredSprints)
            {
                var task = _jira.GetSprintDetails(targetBoard.Id, sprint.Id);
                sprintRequests.Enqueue((sprint, task));
            }

            while (sprintRequests.Any())
            {
                var (sprint, task) = sprintRequests.Dequeue();
                var sprintDetails = task.Result;
                var startJsDate = sprintDetails.SelectToken("startTime")?.Value<long>() ??
                                  throw new InvalidOperationException("Unable to determine sprint start time.");
                var endJsDate = sprintDetails.SelectToken("completeTime")?.Value<long>() ??
                                throw new InvalidOperationException("Unable to determine sprint end time.");

                sprint.Started = ConvertJsTime(startJsDate);
                sprint.Ended = ConvertJsTime(endJsDate);

                var changes = sprintDetails.SelectTokens("changes.*");
                foreach (var change in changes.SelectMany(x => x))
                {
                    var key = change.SelectToken("key")?.Value<string>();
                    var points = change.SelectToken("statC.newValue")?.Value<int>();
                    var isDone = change.SelectToken("column.done")?.Value<bool>();

                    bool? wasUnplanned = null;
                    var addedPath = change.SelectToken("added")?.Path;
                    if (addedPath != null)
                    {
                        var addedTimeText = Regex.Match(addedPath, @"[0-9]{10,}").Value;
                        var addedJsTime = long.Parse(addedTimeText);
                        wasUnplanned = addedJsTime > startJsDate;
                    }

                    var existingIssue = sprint.Issues.FirstOrDefault(x => x.Key == key);
                    if (existingIssue == null)
                    {
                        var issue = new Issue(key);

                        if (points.HasValue)
                            issue.Points = points.Value;
                        if (isDone.HasValue)
                            issue.WasCompleted = isDone.Value;

                        if (wasUnplanned.HasValue)
                            issue.WasUnplanned = wasUnplanned;
                        sprint.Issues.Add(issue);
                    }
                    else
                    {
                        if (points.HasValue)
                            existingIssue.Points = points.Value;
                        if (isDone.HasValue)
                            existingIssue.WasCompleted = isDone.Value;

                        if (wasUnplanned.HasValue)
                            existingIssue.WasUnplanned = wasUnplanned;
                    }
                }
            }

            var unknownPlannedState = filteredSprints.SelectMany(x => x.Issues).Where(x => !x.WasUnplanned.HasValue).Select(x => x.Key).ToList();
            if (unknownPlannedState.Any())
                throw new InvalidOperationException($"Unable to determine whether the following issues were planned or unplanned: {string.Join(',', unknownPlannedState)}");

            var counter = 0;
            var assigneeRequests = new Queue<(Issue, Task<JObject>)>();
            foreach (var issue in filteredSprints.SelectMany(x => x.Issues))
            {
                if (sprintCount > 9 && counter++ % 100 == 0) 
                    await Task.Delay(5000);

                var task = _jira.GetIssueAssignee(issue.Key);
                assigneeRequests.Enqueue((issue, task));
            }

            while (assigneeRequests.Any())
            {
                var (issue, task) = assigneeRequests.Dequeue();
                var issueResponse = task.Result;
                var assignee = issueResponse.SelectToken("fields.assignee.displayName")?.Value<string>();

                if (string.IsNullOrWhiteSpace(assignee))
                    assignee = "Unassigned";

                issue.Assignee = assignee;
            }

            var engine = new PSReportEngine(filteredSprints);
            MemoryCache.Default.Set(cacheKey, engine, CachePolicy);
            return engine;
        }

        private DateTime ConvertJsTime(long jsTime) => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(jsTime);
    }

    public class CapExResult
    {
        public string Key { get; set; }
        public string Assignee { get; set; }
        public double TotalDays { get; set; }
        public string ResolutionDate { get; set; }
        //public DateTime StartDate { get; set; }
        //public DateTime EndDate { get; set; }
        //public double TotalDays => Math.Round((EndDate - StartDate).TotalDays, 1);
    }

    public class IssueTimeline
    {
        private static readonly List<string> _inProgressStatuses = new List<string>
        {
            "In Development",
            "In Validation",
            "In Progress",
            "In Review",
            "Waiting for Engineering",
            "Pending",
            "Escalated",
            "Work in progress",
            "Selected For Remediation",
            "Blocked",
            "Specify",
            "Validation",
            "Waiting for customer",
            "Implementing",
            "Planning",
            "Under investigation",
            "Selected For Acceptance",
            "Remediation In Progress",
            "Risk Not Accepted",
            "Risk Acceptance Expired",
            "Remediation Needed",
            "Ready For Prod",
            "Gathering Feedback"
        };
        private readonly List<IssueStatusChange> _changes;
        //private readonly (IssueStatusChange, int) _changesWithDays;

        public IssueTimeline(IEnumerable<ChangeLogItem> items)
        {
            var may1 = new DateTime(2021, 5, 1);
            _changes = items.OrderBy(x => x.Timestamp).Select(x => new IssueStatusChange
            {
                CurrentStatus = x.Details.Single(y => y.Type == "status").ToStatus, 
                DateTime = x.Timestamp < may1 ? may1 : x.Timestamp
            }).ToList();

            for (var i = 0; i < _changes.Count - 1; i++)
            {
                var days = Math.Round((_changes[i + 1].DateTime - _changes[i].DateTime).TotalDays, 1);
                var weekends = GetWeekendDaysCount(_changes[i].DateTime, _changes[i + 1].DateTime);
                _changes[i].DaysInProgress = days - weekends;
            }
        }

        private int GetWeekendDaysCount(DateTime from, DateTime to)
        {
            var dayDifference = (int)to.Subtract(from).TotalDays;
            return Enumerable
                .Range(1, dayDifference)
                .Select(x => from.AddDays(x))
                .Count(x => x.DayOfWeek == DayOfWeek.Saturday || x.DayOfWeek == DayOfWeek.Sunday);
        }

        public double TotalDaysInProgress()
        {
            var totalDays = 0.0;
            foreach (var change in _changes)
            {
                if (_inProgressStatuses.Contains(change.CurrentStatus))
                {
                    totalDays += change.DaysInProgress;
                }
            }

            return totalDays;
        }
    }

    public class IssueStatusChange
    {
        public DateTime DateTime { get; set; }
        public string CurrentStatus { get; set; }
        public double DaysInProgress { get; set; }
    }
}
