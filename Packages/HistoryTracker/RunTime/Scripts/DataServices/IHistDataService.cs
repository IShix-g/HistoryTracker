
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
        void Remove(HistRecord record);
        HistRecord Set(string recordId, IReadOnlyList<string> paths);
        void Set(HistRecord record, IReadOnlyList<string> paths);
        void Apply(HistRecord record, IReadOnlyList<string> paths, Action<bool> onFinished);
    }
}