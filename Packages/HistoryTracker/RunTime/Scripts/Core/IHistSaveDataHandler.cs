
using System.Collections.Generic;

namespace HistoryTracker
{
    /// <summary>
    /// Provides an interface for handling save data operations,
    /// including saving, retrieving file paths for save data,
    /// and applying save data in a system or application.
    /// </summary>
    public interface IHistSaveDataHandler
    {
        /// <summary>
        /// Called before saving
        /// Note: This is called before data is copied.
        /// </summary>
        HistRecordInfo OnBeforeSave();
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