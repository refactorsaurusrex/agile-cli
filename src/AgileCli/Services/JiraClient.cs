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

                    var wasUnplanned = false;
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
                        issue.WasUnplanned = wasUnplanned;
                        sprint.Issues.Add(issue);
                    }
                    else
                    {
                        if (points.HasValue)
                            existingIssue.Points = points.Value;
                        if (isDone.HasValue)
                            existingIssue.WasCompleted = isDone.Value;
                        existingIssue.WasUnplanned = wasUnplanned;
                    }
                }
            }

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
}
