
using System;

namespace HistoryTracker
{
    /// <summary>
    /// Represents the final data after the application process is complete
    /// </summary>
    public sealed class HistAppliedInfo
    {
        public readonly string Id;
        public readonly string Title;
        public readonly DateTime SavedDateTime;

        public HistAppliedInfo(string id, string title, DateTime savedDateTime)
        {
            Id = id;
            Title = title;
            SavedDateTime = savedDateTime;
        }
    }
}
