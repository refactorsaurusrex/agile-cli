namespace AgileCli.Models
{
    public class Issue
    {
        public Issue(string key) => Key = key;
        public string Key { get; }
        public int Points { get; set; }
        public bool WasCompleted { get; set; }
        public bool WasUnplanned { get; set; }
        public string Assignee { get; set; }
    }
}