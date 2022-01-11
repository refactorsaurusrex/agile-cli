using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace AgileCli.Models
{
    public class JqlQuery
    {
        [JsonProperty("jql")]
        public string Jql { get; set; }

        [JsonProperty("startAt")]
        public int StartAt { get; set; }

        [JsonProperty("maxResults")]
        public int MaxResults => 100;
    }

    public class IssueKeyResponse
    {
        public int StartAt { get; set; }
        public int Total { get; set; }
        public int MaxResults { get; set; }
        public bool IsLast => StartAt + MaxResults >= Total;

        public List<IssueKey> Issues { get; set; }
    }

    [DebuggerDisplay("{Key}")]
    public class IssueKey
    {
        public string Key { get; set; }
        public Fields Fields { get; set; }

        public override string ToString()
        {
            return Key;
        }
    }

    [DebuggerDisplay("{Changes.Count} Items")]
    public class IssueChangeLogResponse
    {
        public bool IsLast { get; set; }

        [JsonProperty("values")]
        public List<ChangeLogItem> Changes { get; set; }

        public override string ToString()
        {
            return $"{Changes.Count} changelog items";
        }
    }

    [DebuggerDisplay("{Timestamp}")]
    public class ChangeLogItem
    {
        [JsonProperty("created")]
        public DateTime Timestamp { get; set; }

        //public Author Author { get; set; }

        [JsonProperty("items")]
        public ChangeDetails[] Details { get; set; }

        public override string ToString()
        {
            return Timestamp.ToString();
        }
    }

    //public class Author 
    //{
    //    public string DisplayName { get; set; }

    //    public override string ToString() => DisplayName;
    //}

    [DebuggerDisplay("{Type}: {FromStatus} --> {ToStatus}")]
    public class ChangeDetails
    {
        [JsonProperty("field")]
        public string Type { get; set; }

        [JsonProperty("fromString")]
        public string FromStatus { get; set; }

        [JsonProperty("toString")]
        public string ToStatus { get; set; }

        public override string ToString()
        {
            return $"{Type}: {FromStatus} --> {ToStatus}";
        }
    }


    public class Fields
    {
        private Assignee _assignee;

        public Assignee Assignee
        {
            get => _assignee;
            set => _assignee = value ?? new Assignee { DisplayName = "Unassigned" };
        }

        //public Status Status { get; set; }

        public override string ToString()
        {
            return $"{Assignee?.DisplayName}";
        }

    }

    //public class Status
    //{
    //    public string Name { get; set; }
    //    public StatusCategory StatusCategory {get; set;}
    //}

    //public class StatusCategory
    //{
    //    public string Key { get; set; }
    //    public string Name { get; set; }
    //}

    public class Assignee
    {
        public string DisplayName { get; set; }
    }


}