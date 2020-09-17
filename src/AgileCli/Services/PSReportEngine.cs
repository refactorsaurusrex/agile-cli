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
    }
}