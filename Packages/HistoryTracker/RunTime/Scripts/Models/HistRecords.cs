
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
        [SerializeField] int _machineId;

        public int Length => _records.Count;
        public bool HasRecords => Length > 0;
        public int MachineId => _machineId;

        public void Initialize()
        {
            _records ??= new List<HistRecord>();
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

        public void Remove(HistRecord record)
        {
            var index = _records.IndexOf(record);
            if (index >= 0)
            {
                _records.RemoveAt(index);
            }
        }

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
