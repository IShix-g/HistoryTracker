
using System.Collections.Generic;

namespace HistoryTracker
{
    /// <summary>
    /// Provides an interface for handling save data operations,
    /// including saving, retrieving file paths for save data,
    /// and applying save data in a system or application.
    /// </summary>
    public interface ISaveDataHandler
    {
        /// <summary>
        /// Save game data
        /// Note: This is called before data is copied.
        /// </summary>
        void SaveData();
        /// <summary>
        /// Get list of save data file paths
        /// Note: Returns a list of file paths required for restoration.
        /// </summary>
        IReadOnlyList<string> GetSaveFilePaths();
        /// <summary>
        /// Called after save data is applied
        /// Note: Typically used for operations like exiting the application.
        /// </summary>
        void ApplyData();
    }
}