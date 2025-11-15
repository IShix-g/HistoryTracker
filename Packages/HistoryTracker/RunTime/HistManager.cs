
using System;

namespace HistoryTracker
{
    internal sealed class HistManager
    {
        public event Action<HistRecord> OnAddRecord = delegate {};
        public event Action<HistRecord> OnRemoveRecord = delegate {};
        
        public HistRecords Records => _service.GetRecords();

        readonly IHistSaveDataHandler _handler;
        readonly IHistDataService _service;

        public HistManager(
            IHistSaveDataHandler handler,
            IHistDataService service
        )
        {
            _handler = handler;
            _service = service;
        }

        public void Save() => _service.Save();

        public void Delete() => _service.Delete();

        public void DeleteAll() => _service.DeleteAll();

        public HistRecord SaveHistory()
        {
            var paths = _handler.GetSaveFilePaths();
            var info = _handler.OnBeforeSave();
            var record = _service.Add(paths);
            record.Title = info.Title;
            record.Description = info.Description;
            OnAddRecord(record);
            return record;
        }

        public void Remove(HistRecord record)
        {
            _service.Remove(record);
            OnRemoveRecord(record);
        }

        public void Apply(HistRecord record)
        {
            var paths = _handler.GetSaveFilePaths();
            _service.Apply(record, paths);
            _handler.ApplyData();
        }
    }
}