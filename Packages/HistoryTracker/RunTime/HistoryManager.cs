
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HistoryTracker
{
    public sealed class HistoryManager
    {
        public HistoryRecords Records { get; private set; }

        readonly ISaveDataHandler _dataHandler;
        readonly IHistoryDataService _service;

        public HistoryManager(
            ISaveDataHandler dataHandler,
            IHistoryDataService service
        )
        {
            _dataHandler = dataHandler;
            _service = service;
        }
        
        public void Save() => _service.Save();
        
        public void Delete() => _service.Delete();
        
        public void DeleteAll() => _service.DeleteAll();
        
        public HistoryRecord Add(IReadOnlyList<string> paths)
        {
            _dataHandler.SaveData();
            return _service.Add(paths);
        }
        
        public void Remove(HistoryRecord record) => _service.Remove(record);
        
        public void Apply(HistoryRecord record)
        {
            var paths = _dataHandler.GetSaveFilePaths();

            foreach (var path in paths)
            {
                var fileName = record.Paths.FirstOrDefault(x => path.Contains(Path.GetFileName(x)));
            }
            
            _dataHandler.ApplyData();
        }
    }
}