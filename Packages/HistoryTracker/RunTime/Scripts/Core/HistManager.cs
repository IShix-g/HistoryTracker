
using System;
using System.IO;

namespace HistoryTracker
{
    internal sealed class HistManager
    {
        public event Action OnStartSave = delegate {};
        public event Action OnEndSave = delegate {};
        public event Action OnStartAddRecord = delegate {};
        public event Action OnEndAddRecord = delegate {};
        public event Action<HistRecord> OnAddRecord = delegate {};
        public event Action<HistRecord> OnRemoveRecord = delegate {};
        public event Action<HistRecord> OnRestoreFromTrash = delegate {};
        public event Action OnEmptyTrash = delegate {};
        public event Action OnStartApply = delegate {};
        public event Action<HistAppliedInfo> OnEndApply = delegate {};

        public HistRecords Records => _service.GetRecords();
        public bool IsStartApply { get; private set; }

        IHistSaveDataHandler _handler;
        IHistDataService _service;

        public void Initialize(IHistSaveDataHandler handler, IHistDataService service)
        {
            _handler = handler;
            _service = service;
        }

        public void Save()
        {
            OnStartSave();
            _service.Save(() => OnEndSave());
        }

        public void Delete() => _service.Delete();

        public void DeleteAll() => _service.DeleteAll();

        public void SaveHistory(HistRecordInfo addInfo = null, HistRecordInfoPlacement placement = HistRecordInfoPlacement.Prepend, Action<HistRecord> added = null)
        {
            OnStartAddRecord();
            var info = _handler.OnBeforeSave();
            var paths = _handler.GetSaveFilePaths();
            _service.Add(paths, record =>
            {
                record.Title = MergeRecordText(placement, info.Title, addInfo?.Title);
                record.Description = MergeRecordText(placement, info.Description, addInfo?.Description) + "\nPaths:\n" + ToNormalizedPathString(record);
                added?.Invoke(record);
                OnAddRecord(record);
                OnEndAddRecord();
            });
        }

        public void Apply(HistRecord record, Action<bool> completed = null)
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
                var info = default(HistAppliedInfo);
                if (isSuccess)
                {
                    var savedDate = DateTime.Parse(record.TimeStamp);
                    info = new HistAppliedInfo(record.Id, record.Title, savedDate);
                    _handler.ApplyData(info);
                }
                completed?.Invoke(isSuccess);
                OnEndApply(info);
            });
        }

        public void MoveToTrash(HistRecord record)
        {
            _service.MoveToTrash(record);
            OnRemoveRecord(record);
        }

        public void RestoreFromTrash(HistRecord record)
        {
            _service.RestoreFromTrash(record);
            OnRestoreFromTrash(record);
        }

        public void EmptyTrash()
        {
            if (!_service.GetRecords().HasTrashRecords)
            {
                return;
            }
            _service.EmptyTrash();
            OnEmptyTrash();
        }

        string MergeRecordText(HistRecordInfoPlacement placement, string baseText, string addText)
        {
            if (string.IsNullOrEmpty(addText))
            {
                return baseText;
            }
            if (string.IsNullOrEmpty(baseText))
            {
                return addText;
            }
            return placement == HistRecordInfoPlacement.Prepend
                ? $"{addText}\n{baseText}"
                : $"{baseText}\n{addText}";
        }

        string ToNormalizedPathString(HistRecord record)
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
