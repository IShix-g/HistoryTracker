
using System.IO;
using UnityEngine;

namespace HistoryTracker
{
    internal sealed class EditorHistDataService : FileBasedHistDataService
    {
        public EditorHistDataService(string dirName)
            : base(GetDataPath(dirName)) {}

        public static string GetDataPath(string dirName)
            => Path.Combine(Path.GetDirectoryName(Application.dataPath), dirName, Hist.RecordsFileName);
    }
}