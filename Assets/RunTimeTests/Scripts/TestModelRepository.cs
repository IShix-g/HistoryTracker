#if DEBUG
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HistoryTracker;
using UnityEngine;

namespace Tests
{
    public sealed class TestModelRepository : ModelRepository, IHistSaveDataHandler
    {
        HistRecordInfo IHistSaveDataHandler.OnBeforeSave()
        {
            var title = string.Empty;
            var description = string.Empty;
            var persistentDataPathLength = Application.persistentDataPath.Length;
            for (var i = 0; i < Models.Count; i++)
            {
                var model = Models[i];
                var path = GetFullPath(model.Id);
                Save(model, path);
                var relativePath = path.Substring(persistentDataPathLength)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                description += $"-{relativePath} \n";
            }
            title += "Saved Count: " + Models[0].SaveCount;
            return new HistRecordInfo(title, description);
        }

        IReadOnlyList<string> IHistSaveDataHandler.GetSaveFilePaths() => Paths.Values.ToList();

        void IHistSaveDataHandler.ApplyData() => Restored();
    }
}
#endif