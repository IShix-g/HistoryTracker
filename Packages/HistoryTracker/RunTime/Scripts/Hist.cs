
using System;

namespace HistoryTracker
{
    public static class Hist
    {
        public const string DirectoryName = "HistoryTrackerRecords";
        public const string RecordsFileName = "Records.dat";

        static HistManager s_manager;

        public static void Configure(IHistSaveDataHandler handler)
        {
            s_manager ??= Create(handler);
#if !UNITY_EDITOR
            if (HistSettings.HasSettings
                && HistSettings.Current.IncludeRecordsInBuild)
            {
                StreamingAssetsRecordsInstaller.Install(s_manager);
            }
#endif
        }

        static HistManager Create(IHistSaveDataHandler handler)
        {
#if UNITY_EDITOR
            var service = new EditorHistDataService(DirectoryName);
#else
            var service = new PersistentHistDataService(DirectoryName);
#endif
            s_manager = new HistManager(handler, service);
            return s_manager;
        }

        public static HistUI CreateOrGet()
        {
            if (s_manager == null)
            {
                throw new InvalidOperationException("[HistoryTracker] `Hist.Configure()` must be called before using HistoryTracker");
            }
            return HistUI.CreateOrGet(s_manager);
        }

        public static void Release() => HistUI.Release();
    }
}