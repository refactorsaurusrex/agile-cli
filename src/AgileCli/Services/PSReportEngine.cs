using System.Collections.Generic;
using System.Linq;
using AgileCli.Models;

namespace AgileCli.Services
{
    public class PSReportEngine
    {
        private readonly ICollection<Sprint> _sprints;

        public PSReportEngine(ICollection<Sprint> sprints) => _sprints = sprints;

        public object GetVelocityAverages() => new
        {
            AverageCommitted = (int)_sprints.Average(x => x.CommittedPoints),
            AverageCompleted = (int)_sprints.Average(x => x.CompletedPoints),
            AverageRollover = (int)_sprints.Average(x => x.RolloverPoints),
            AverageUnplanned = (int)_sprints.Average(x => x.UnplannedPoints)
        };

        public object GetVelocityReport() =>
            _sprints
                .OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    x.Name,
                    x.Started,
                    x.Ended,
                    Committed = x.CommittedPoints,
                    Unplanned = x.UnplannedPoints,
                    Completed = x.CompletedPoints,
                    Rollover = x.RolloverPoints,
                    PercentRollover = x.PercentRollover.ToString("p0")
                });

        public string GetPercentOnTargetSprints(double target)
        {
            var total = _sprints.Count;
            var offTarget = _sprints.Count(x => x.PercentRollover > target || x.PercentRollover < target * -1);
            return ((total - offTarget) / (double)total).ToString("p0");
        }

        public IEnumerable<dynamic> GetFormattedUserAverages()
        {
            return GetRawUserAverages().Select(x => new
            {
                x.Assignee,
                AverageCommitted = x.AverageCommitted.ToString("N1"),
                AverageCompleted = x.AverageCompleted.ToString("N1"),
                AverageRollover = x.AverageRollover.ToString("N1"),
                RolloverPercent = x.RolloverPercent.ToString("p0")
            });
        }

        public IEnumerable<dynamic> GetRawUserAverages()
        {
            return _sprints
                .SelectMany(sprint => sprint.Issues)
                .GroupBy(issue => issue.Assignee)
                .Select(grouping =>
                {
                    var committed = grouping.Sum(issue => issue.Points) / (double)_sprints.Count;
                    var rollover = grouping.Where(issue => !issue.WasCompleted).Sum(issue => issue.Points) / (double)_sprints.Count;
                    return new
                    {
                        Assignee = grouping.Key,
                        AverageCommitted = committed,
                        AverageCompleted = (grouping.Where(issue => issue.WasCompleted).Sum(issue => issue.Points) / (double)_sprints.Count),
                        AverageRollover = rollover,
                        RolloverPercent = committed == 0.0 ? 0.0 : rollover / committed
                    };
                })
                .OrderBy(x => x.Assignee);
        }
    }
}