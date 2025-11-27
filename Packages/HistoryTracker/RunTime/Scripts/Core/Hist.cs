
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
                LocaleProvider.Initialize();
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
        /// <param name="addInfo">Additional record information to save, such as a title or description.</param>
        /// <param name="placement">Specifies the position at which to insert the history record.</param>
        public static void SaveHistory(HistRecordInfo addInfo = null,
            HistRecordInfoPlacement placement = HistRecordInfoPlacement.Prepend)
        {
            if (HistSettings.Current.IsScopeActive
                && s_manager != null)
            {
                s_manager.SaveHistory(addInfo, placement);
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

        /// <summary>
        /// Creates or retrieves the singleton instance of the HistoryTracker UI.
        /// This method initializes and returns the UI associated with the provided HistManager.
        /// Requires <c>Hist.Configure()</c> to be called prior to its usage.
        /// </summary>
        /// <returns>The active instance of the HistoryTracker UI.</returns>
        public static HistUI CreateOrGetUI()
        {
            if (s_manager == null)
            {
                var errorMsg = HistSettings.Current.IsScopeActive ?
                    "[HistoryTracker] `Hist.Configure()` must be called before using HistoryTracker"
                    : "[HistoryTracker] Cannot display dialog because it is outside the ActivationScope range. ActivationScope can be changed in settings: Window > HistoryTracker > open Settings";
                throw new InvalidOperationException(errorMsg);
            }
            return HistUI.CreateOrGet(s_manager);
        }

        public static void Release() => HistUI.Release();

        /// <summary>
        /// Opens the History dialog.
        /// </summary>
        /// <param name="autoRelease">Whether to release resources when the dialog closes.</param>
        /// <returns>The active instance of the HistoryTracker UI.</returns>
        public static HistUI OpenDialog(bool autoRelease = false)
        {
            var ui = CreateOrGetUI();
            ui.OpenDialog(autoRelease ? Release : null);
            return ui;
        }
    }
}
