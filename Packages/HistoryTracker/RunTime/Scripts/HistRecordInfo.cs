
namespace HistoryTracker
{
    /// <summary>
    /// Information displayed in the UI list
    /// </summary>
    public sealed class HistRecordInfo
    {
        public readonly string Title;
        public readonly string Description;

        public HistRecordInfo(string title, string description)
        {
            Title = title;
            Description = description;
        }
    }
}
