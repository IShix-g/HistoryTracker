
namespace HistoryTracker
{
    public static class Hist
    {
        internal static HistManager Manager { get; private set; }
        
        public static void Configure(IHistSaveDataHandler handler)
            => Manager ??= Create(handler);

        static HistManager Create(IHistSaveDataHandler handler)
            => Manager = new HistManager(handler, new PersistentHistDataService("HistoryTracker"));
    }
}