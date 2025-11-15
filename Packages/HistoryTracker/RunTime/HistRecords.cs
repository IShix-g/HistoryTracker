
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace HistoryTracker
{
    [Serializable]
    internal sealed class HistRecords : IEnumerable<HistRecord>
    {
        [SerializeField] List<HistRecord> _records = new ();

        public int Length => _records.Count;
        public bool HasRecords => Length > 0;
 
        public bool HasRecord(int id) => GetById(id) != null;        

        public HistRecord GetAt(int index) => _records[index];
        
        public HistRecord GetRevirseAt(int index) => _records[(_records.Count - 1) - index];

        public HistRecord GetById(int id) => _records.Find(x => x.Id == id);

        public HistRecord Create()
        {
            var record = new HistRecord(Length + 1)
            {
                TimeStamp = DateTime.Now.ToUniversalTime().ToString("o")
            };
            _records.Add(record);
            return record;
        }

        public HistRecord GetOrCreateById(int id)
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
        public int Id;
        public string Title;
        public string Description;
        public List<string> Paths = new ();
        public string TimeStamp;

        public HistRecord(int id)
        {
            Assert.IsTrue(id > 0);
            Id = id;
        }
    }
}