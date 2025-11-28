
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace HistoryTracker
{
    public static class Hist
    {
        public const string DirectoryName = "HistoryTrackerRecords";
        public const string RecordsFileName = "Records.dat";

        static readonly HistManager s_manager = new ();
        static IHistDataService s_dataService;

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
            if (!HistSettings.Current.IsScopeActive)
            {
                return;
            }
            if (!LocaleProvider.IsInitialized)
            {
                LocaleProvider.Initialize();
            }
#if UNITY_EDITOR
            s_dataService ??= new EditorHistDataService(DirectoryName);
#else
            s_dataService ??= new PersistentHistDataService(DirectoryName);
#endif
            s_manager.Initialize(handler, s_dataService);

#if !UNITY_EDITOR
            if (HistSettings.Current.IncludeRecordsInBuild)
            {
                StreamingAssetsRecordsInstaller.Install(s_manager);
            }
#endif

#if DEBUG
            ValidSaveFilePathsAsync(handler).Handled();
#endif
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

        static async Task ValidSaveFilePathsAsync(IHistSaveDataHandler handler)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            if (handler != null)
            {
                var paths = handler.GetSaveFilePaths();
                ValidSaveFilePaths(paths);
            }
        }

        /// <summary>
        /// Checks a list of paths to ensure none of them are EXISTING directories (folders).
        /// This validation is performed at startup, and the paths may not yet exist on the file system.
        /// Paths that are currently files or do not exist are considered valid, as they are intended
        /// to be used as file save paths later.
        /// </summary>
        /// <param name="paths">list of save data file paths</param>
        static void ValidSaveFilePaths(IReadOnlyList<string> paths)
        {
            var errorMsg = string.Empty;
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    errorMsg += "- Cannot specify a directory: " + path + "\n";
                }
            }

            if (!string.IsNullOrEmpty(errorMsg))
            {
                Debug.LogError("[HistoryTracker] Invalid save file paths: \n" + errorMsg);
            }
        }
    }
}
