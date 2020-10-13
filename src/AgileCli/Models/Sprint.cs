using System;
using System.Linq;

namespace AgileCli.Models
{
    public class Sprint
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Goal { get; set; }
        public string State { get; set; }
        public DateTime Started { get; set; }
        public DateTime Ended { get; set; }
        public IssueCollection Issues { get; } = new IssueCollection();

        public int CompletedPoints => Issues.Where(x => x.WasCompleted).Sum(x => x.Points);
        public int CommittedPoints => Issues.Sum(x => x.Points);
        // ReSharper disable once PossibleInvalidOperationException
        public int UnplannedPoints => Issues.Where(x => x.WasUnplanned.Value).Sum(x => x.Points);
        public int RolloverPoints => Issues.Where(x => !x.WasCompleted).Sum(x => x.Points);
        public double PercentRollover => RolloverPoints / (double)CommittedPoints;
    }
}