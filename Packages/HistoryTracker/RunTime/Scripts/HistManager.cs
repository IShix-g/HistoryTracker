
using System;
using System.IO;

namespace HistoryTracker
{
    internal sealed class HistManager
    {
        public event Action<HistRecord> OnAddRecord = delegate {};
        public event Action<HistRecord> OnRemoveRecord = delegate {};
        public event Action OnStartApply = delegate {};
        public event Action OnEndApply = delegate {};
        
        public HistRecords Records => _service.GetRecords();
        public bool IsStartApply { get; private set; }
        
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
            record.Description += "\n" + GetPathList(record);
            OnAddRecord(record);
            return record;
        }

        public void Remove(HistRecord record)
        {
            _service.Remove(record);
            OnRemoveRecord(record);
        }

        public void Apply(HistRecord record, Action<bool> onFinished = null)
        {
            if (IsStartApply)
            {
                return;
            }
            IsStartApply = true;
            var paths = _handler.GetSaveFilePaths();
            OnStartApply();
            _service.Apply(record, paths, isSuccess =>
            {
                IsStartApply = false;
                onFinished?.Invoke(isSuccess);
                OnEndApply();
                if (isSuccess)
                {
                    _handler.ApplyData();
                }
            });
        }

        string GetPathList(HistRecord record)
        {
            var stg = string.Empty;
            foreach (var path in record.Paths)
            {
                var normalizedPath = path.Substring(record.Id.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                stg += "- " + normalizedPath + "\n";
            }
            return stg;
        }
    }
}