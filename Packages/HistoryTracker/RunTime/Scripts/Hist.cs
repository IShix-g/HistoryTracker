
using System;

namespace HistoryTracker
{
    public static class Hist
    {
        static HistManager s_manager;

        public static void Configure(IHistSaveDataHandler handler)
            => s_manager ??= Create(handler);

        static HistManager Create(IHistSaveDataHandler handler)
        {
#if UNITY_EDITOR
            var service = new EditorHistDataService("HistoryTrackerRecords");
#else
            var service = new PersistentHistDataService("HistoryTrackerRecords");
#endif
            s_manager = new HistManager(handler, service);
            return s_manager;
        }

        public static HistUI CreateOrGet()
        {
            if (s_manager == null)
            {
                throw new InvalidOperationException("`Hist.Configure()` must be called before using HistoryTracker");
            }
            return HistUI.CreateOrGet(s_manager);
        }

        public static void Release() => HistUI.Release();
    }
}