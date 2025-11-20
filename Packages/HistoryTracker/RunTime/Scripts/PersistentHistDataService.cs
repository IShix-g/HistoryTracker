
using System.IO;
using UnityEngine;

namespace HistoryTracker
{
    internal sealed class PersistentHistDataService : FileBasedHistDataService
    {
        public PersistentHistDataService(string dirName)
            : base(GetDataPath(dirName)) {}
        
        public static string GetDataPath(string dirName)
            => Path.Combine(Application.persistentDataPath, dirName, Hist.RecordsFileName);
    }
}