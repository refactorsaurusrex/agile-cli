﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AgileCli.Api;
using AgileCli.Infrastructure;
using AgileCli.Models;
using AsyncProgressReporter;
using Newtonsoft.Json.Linq;

namespace AgileCli.Services
{
    public class JiraClient
    {
        private readonly IJiraApi _jira;
        private static readonly CacheItemPolicy Policy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(60) };

        public JiraClient(string jiraHostname, string accessToken)
        {
            _jira = AuthenticatedRestService.For<IJiraApi>(accessToken, $"https://{jiraHostname}");
        }

        public bool DisableCache { get; set; }

        public async Task<IEnumerable<Board>> GetBoards()
        {
            if (!DisableCache && MemoryCache.Default["GetBoards"] is List<Board> boards)
                return boards;

            var boardsResponse = await _jira.GetBoards();
            return boardsResponse.Boards;
        }

        public async Task<PSReportEngine> CreateSprintReportEngine(string boardName, int sprintCount, IProgressReporter reporter, bool getAssignees)
        {
            var cacheKey = $"PSReportEngine{boardName}{sprintCount}{getAssignees}";
            if (!DisableCache && MemoryCache.Default[cacheKey] is PSReportEngine cachedEngine)
            {
                reporter.CompleteAdding();
                return cachedEngine;
            }

            try
            {
                await reporter.UpdateProgress("Getting Jira board data...", 0, 1);
                var allBoardsResponse = await _jira.GetBoards();
                var targetBoard = allBoardsResponse.Boards.SingleOrDefault(x => x.Name.Equals(boardName, StringComparison.InvariantCultureIgnoreCase)) ??
                                  throw new InvalidOperationException($"Unable to locate the board '{boardName}'.");

                var sprintCollection = await _jira.GetSprints(targetBoard.Id);
                var filteredSprints = sprintCollection.ClosedSprints.Take(sprintCount).ToList();
                var total = filteredSprints.Count;
                var count = 0;

                foreach (var sprint in filteredSprints)
                {
                    await reporter.UpdateProgress($"Pulling data for spring {sprint.Name}...", ++count, total);
                    var sprintDetails = await _jira.GetSprintDetails(targetBoard.Id, sprint.Id);
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

                            //if (getAssignees)
                            //{
                            //    var issueResponse = await _jira.GetIssueAssignee(key);
                            //    var assignee = issueResponse.SelectToken("fields.assignee.displayName")?.Value<string>();

                            //    if (string.IsNullOrWhiteSpace(assignee))
                            //        throw new InvalidOperationException($"Unable to retrieve assignee name for issue {key}");

                            //    issue.Assignee = assignee;
                            //}

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

                if (getAssignees)
                {
                    var assigneeRequests = new List<(Issue, Task<JObject>)>();
                    foreach (var issue in filteredSprints.SelectMany(x => x.Issues))
                    {
                        var task = _jira.GetIssueAssignee(issue.Key);
                        assigneeRequests.Add((issue, task));
                    }

                    foreach (var (issue, task) in assigneeRequests)
                    {
                        var issueResponse = task.Result;
                        var assignee = issueResponse.SelectToken("fields.assignee.displayName")?.Value<string>();

                        if (string.IsNullOrWhiteSpace(assignee))
                            throw new InvalidOperationException($"Unable to retrieve assignee name for issue {issue.Key}");

                        issue.Assignee = assignee;
                    }
                }

                var engine = new PSReportEngine(filteredSprints);
                MemoryCache.Default.Set(cacheKey, engine, Policy);
                return engine;
            }
            finally
            {
                reporter.CompleteAdding();
            }
        }

        private DateTime ConvertJsTime(long jsTime) => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(jsTime);
    }
}
