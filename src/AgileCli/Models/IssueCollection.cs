using System.Collections.ObjectModel;

namespace AgileCli.Models
{
    public class IssueCollection : KeyedCollection<string, Issue>
    {
        protected override string GetKeyForItem(Issue item) => item.Key;
    }
}