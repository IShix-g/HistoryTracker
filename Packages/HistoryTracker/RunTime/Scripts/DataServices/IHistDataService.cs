
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
        void Apply(HistRecord record, IReadOnlyList<string> paths, Action<bool> onFinished);
        void MoveToTrash(HistRecord record);
        void RestoreFromTrash(HistRecord record);
        void EmptyTrash();
    }
}
