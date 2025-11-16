
using System.IO;
using UnityEngine;

namespace HistoryTracker
{
    internal sealed class PersistentHistDataService : FileBasedHistDataService
    {
        public PersistentHistDataService(string dirName)
            : base(
                Path.Combine(Application.persistentDataPath, dirName),
                Path.Combine(Application.persistentDataPath, dirName, "Records.dat")
            ) {}
    }
}