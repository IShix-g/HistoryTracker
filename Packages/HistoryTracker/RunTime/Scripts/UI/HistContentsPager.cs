
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HistoryTracker
{
    internal sealed class HistContentsPager
    {
        public int Length { get; private set; }
        public int OnceContentsLength { get; }

        public bool HasNext => _currentIndex < _indexList.Count - 1;
        public bool HasPrev => _currentIndex > 0;
        public int StartIndex => _indexList.Count > 0 ? _indexList[_currentIndex].StartIndex : 0;
        public int EndIndex => _indexList.Count > 0 ? _indexList[_currentIndex].EndIndex : 0;

        readonly List<Index> _indexList = new ();
        int _currentIndex;

        sealed class Index
        {
            public int StartIndex;
            public int EndIndex;
        }

        public HistContentsPager(int length, int onceContentsLength)
        {
            Length = length;
            OnceContentsLength = onceContentsLength;
            RebuildIndexes();
        }

        public void UpdateLength(int newLength)
        {
            if (newLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newLength));
            }
            Length = newLength;
            RebuildIndexes();
        }

        public void Next() => _currentIndex++;

        public void Prev() => _currentIndex--;
        
        void RebuildIndexes()
        {
            _indexList.Clear();
            for (var i = 0; i < Length; i += OnceContentsLength)
            {
                _indexList.Add(new Index
                {
                    StartIndex = i,
                    EndIndex = Mathf.Min(i + OnceContentsLength - 1, Length - 1)
                });
            }
            
            if (_indexList.Count == 0)
            {
                _currentIndex = 0;
            }
            else if (_currentIndex >= _indexList.Count)
            {
                _currentIndex = _indexList.Count - 1;
            }
        }
    }
}