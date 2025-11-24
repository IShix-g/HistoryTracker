
using System;

namespace HistoryTracker
{
    public static class Hist
    {
        public const string DirectoryName = "HistoryTrackerRecords";
        public const string RecordsFileName = "Records.dat";

        static HistManager s_manager;

        /// <summary>
        /// Configure HistoryTracker.
        /// Please initialize as early as possible during game startup, such as in Awake.
        /// </summary>
        /// <param name="handler">Associates your save system with HistoryTracker via IHistSaveDataHandler.</param>
        /// <example>
        /// <code>
        /// using HistoryTracker;
        ///
        /// void Awake()
        /// {
        ///     Hist.Configure(_repository);
        /// }
        /// </code>
        /// </example>
        public static void Configure(IHistSaveDataHandler handler)
        {
            if (HistSettings.Current.IsScopeActive)
            {
                s_manager ??= Create(handler);
#if !UNITY_EDITOR
                if (HistSettings.Current.IncludeRecordsInBuild)
                {
                    StreamingAssetsRecordsInstaller.Install(s_manager);
                }
#endif
            }
        }

        /// <summary>
        /// Save the history of saved games
        /// Performs the same action as the dialog's Save button.
        /// </summary>
        /// <param name="addInfo"></param>
        public static void SaveHistory(HistRecordInfo addInfo = null)
        {
            if (HistSettings.Current.IsScopeActive
                && s_manager != null)
            {
                s_manager.SaveHistory(addInfo);
                s_manager.Save();
            }
        }

        static HistManager Create(IHistSaveDataHandler handler)
        {
#if UNITY_EDITOR
            var service = new EditorHistDataService(DirectoryName);
#else
            var service = new PersistentHistDataService(DirectoryName);
#endif
            return s_manager = new HistManager(handler, service);
        }

        public static HistUI CreateOrGetUI()
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
