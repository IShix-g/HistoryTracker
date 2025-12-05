
using System;
using System.Collections.Generic;

namespace HistoryTracker
{
    internal interface IHistDataService
    {
        HistRecords GetRecords();
        void Save(Action completed);
        void Delete();
        void DeleteAll();

        void Add(IReadOnlyList<string> paths, Action<HistRecord> completed);
        void Apply(HistRecord record, IReadOnlyList<string> paths, Action<bool> onFinished);
        void MoveToTrash(HistRecord record);
        void RestoreFromTrash(HistRecord record);
        void EmptyTrash();
    }
}
