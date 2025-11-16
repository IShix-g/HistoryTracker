
using System.IO;
using UnityEngine;

namespace HistoryTracker
{
    internal sealed class EditorHistDataService : FileBasedHistDataService
    {
        public EditorHistDataService(string dirName)
            : base(
                Path.Combine(Path.GetDirectoryName(Application.dataPath), dirName),
                Path.Combine(Path.GetDirectoryName(Application.dataPath), dirName, "Records.dat")
            ) {}
    }
}