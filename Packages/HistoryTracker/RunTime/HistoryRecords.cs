
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace HistoryTracker
{
    [Serializable]
    public sealed class HistoryRecords : IEnumerable<HistoryRecord>
    {
        public event Action<HistoryRecord> OnAdd = delegate {};
        public event Action<HistoryRecord> OnRemove = delegate {};

        [SerializeField] List<HistoryRecord> _records = new ();

        public int Length => _records.Count;
        public bool HasRecords => Length > 0;
 
        public bool HasRecord(int id) => GetById(id) != null;        

        public HistoryRecord GetAt(int index) => _records[index];

        public HistoryRecord GetById(int id) => _records.Find(x => x.Id == id);

        public HistoryRecord Create()
        {
            var record = new HistoryRecord(Length + 1)
            {
                TimeStamp = DateTime.Now.ToUniversalTime().ToString("o")
            };
            _records.Add(record);
            OnAdd(record);
            return record;
        }

        public HistoryRecord GetOrCreateById(int id)
        {
            var record = GetById(id);
            return record ?? Create();
        }

        public void Remove(HistoryRecord record)
        {
            var index = _records.IndexOf(record);
            if (index >= 0)
            {
                _records.RemoveAt(index);
                OnRemove(record);
            }
        }

        public IEnumerator<HistoryRecord> GetEnumerator() => _records.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    
    [Serializable]
    public sealed class HistoryRecord
    {
        public int Id;
        public string Title;
        public string Description;
        public List<string> Paths = new ();
        public string TimeStamp;

        public HistoryRecord(int id)
        {
            Assert.IsTrue(id > 0);
            Id = id;
        }
    }
}