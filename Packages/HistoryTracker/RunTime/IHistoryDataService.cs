
using System.Collections.Generic;

namespace HistoryTracker
{
    public interface IHistoryDataService
    {
        HistoryRecords GetRecords();
        void Save();
        void Delete();
        void DeleteAll();
        
        HistoryRecord Add(IReadOnlyList<string> paths);
        void Remove(HistoryRecord record);
        HistoryRecord Set(int recordId, IReadOnlyList<string> paths);
        void Set(HistoryRecord record, IReadOnlyList<string> paths);
        void Apply(HistoryRecord record, IReadOnlyList<string> paths);
    }
}