
using System;
using System.Collections.Generic;

namespace HistoryTracker
{
    internal interface IHistDataService
    {
        HistRecords GetRecords();
        void Save();
        void Delete();
        void DeleteAll();

        HistRecord Add(IReadOnlyList<string> paths);
        HistRecord Set(string recordId, IReadOnlyList<string> paths);
        void Set(HistRecord record, IReadOnlyList<string> paths);
        void Apply(HistRecord record, IReadOnlyList<string> paths, Action<bool> onFinished);

        IReadOnlyList<HistRecord> GetTrashRecords();
        void MoveToTrash(HistRecord record);
        void RestoreFromTrash(HistRecord record);
        void EmptyTrash();
    }
}
