
using System.IO;
using UnityEngine;

namespace HistoryTracker
{
    internal sealed class EditorHistDataService : FileBasedHistDataService
    {
        static readonly string s_rootDir = Path.GetDirectoryName(Application.dataPath);
        
        public EditorHistDataService(string dirName)
            : base(
                Path.Combine(s_rootDir, dirName),
                Path.Combine(s_rootDir, dirName, "Records.dat")
            ) {}
    }
}