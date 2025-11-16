
using System.IO;
using UnityEngine;

namespace HistoryTracker
{
    internal sealed class PersistentHistDataService : FileBasedHistDataService
    {
        static readonly string s_rootDir = Application.persistentDataPath;
        
        public PersistentHistDataService(string dirName)
            : base(
                Path.Combine(s_rootDir, dirName),
                Path.Combine(s_rootDir, dirName, "Records.dat")
            ) {}
    }
}