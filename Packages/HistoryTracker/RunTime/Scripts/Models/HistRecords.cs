
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = System.Random;

namespace HistoryTracker
{
    [Serializable]
    internal sealed class HistRecords : IEnumerable<HistRecord>
    {
        [SerializeField] List<HistRecord> _records;
        [SerializeField] List<HistRecord> _trashRecords;
        [SerializeField] int _machineId;

        public int Length => _records.Count;
        public int TrashLength => _trashRecords.Count;
        public bool HasRecords => Length > 0;
        public bool HasTrashRecords => TrashLength > 0;
        public int MachineId => _machineId;

        public void Initialize()
        {
            _records ??= new List<HistRecord>();
            _trashRecords ??= new List<HistRecord>();
            if (_machineId == 0)
            {
                SetNewMachineId();
            }
        }

        public void SetNewMachineId()
        {
            var guid = Guid.NewGuid();
            var seed = guid.GetHashCode();
            var random = new Random(seed);
            _machineId = random.Next(1, 1_000_000);
        }

        public bool HasRecord(string id) => GetById(id) != null;

        public HistRecord GetAt(int index) => _records[index];

        public HistRecord GetReverseAt(int index) => _records[(_records.Count - 1) - index];

        public HistRecord GetFromTrashAt(int index) => _trashRecords[index];

        public HistRecord GetFromTrashReverseAt(int index) => _trashRecords[(_trashRecords.Count - 1) - index];

        public HistRecord GetById(string id) => _records.Find(x => x.Id == id);

        public HistRecord Create()
        {
            var id = IdGenerator.NewId(_machineId);
            var record = new HistRecord(id)
            {
                TimeStamp = DateTime.Now.ToUniversalTime().ToString("o")
            };
            _records.Add(record);
            return record;
        }

        public void AddOrSet(HistRecord record)
        {
            Assert.IsFalse(string.IsNullOrEmpty(record.Id));
            var index = _records.FindIndex(x => x.Id == record.Id);
            if (index >= 0)
            {
                _records[index] = record;
            }
            else
            {
                _records.Add(record);
            }
        }

        public HistRecord GetOrCreateById(string id)
        {
            var record = GetById(id);
            return record ?? Create();
        }

        public void MoveToTrash(HistRecord record)
        {
            var index = _records.IndexOf(record);
            if (index >= 0)
            {
                _records.RemoveAt(index);
                _trashRecords.Add(record);
            }
            else
            {
                throw new InvalidOperationException($"Record with id={record.Id} not found");
            }
        }

        public void RestoreFromTrash(HistRecord record)
        {
            var index = _trashRecords.IndexOf(record);
            if (index >= 0)
            {
                _trashRecords.RemoveAt(index);
                _records.Add(record);
                _records.Sort((a, b) => string.Compare(a.TimeStamp, b.TimeStamp, StringComparison.Ordinal));
            }
            else
            {
                throw new InvalidOperationException($"Record with id={record.Id} not found");
            }
        }

        public void EmptyTrash() => _trashRecords.Clear();

        public IReadOnlyList<HistRecord> GetTrashRecords() => _trashRecords;

        public IEnumerator<HistRecord> GetEnumerator() => _records.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [Serializable]
    internal sealed class HistRecord
    {
        public string Id;
        public string Title;
        public string Description;
        public List<string> Paths = new ();
        public string TimeStamp;
        public bool IsStreamingAssets;

        public HistRecord(string id)
        {
            Assert.IsFalse(string.IsNullOrEmpty(id));
            Id = id;
        }
    }
}
