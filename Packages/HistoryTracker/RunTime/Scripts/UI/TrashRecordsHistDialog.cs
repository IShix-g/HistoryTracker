
using System;
using UnityEngine;
using UnityEngine.UI;

namespace HistoryTracker
{
    internal sealed class TrashRecordsHistDialog : HistDialog
    {
        [SerializeField] Button _closeButton;
        [SerializeField] Button _nextButton;
        [SerializeField] Button _prevButton;
        [SerializeField] Button _emptyTrashButton;
        [SerializeField] Text _pagerText;
        [SerializeField] TrashRecordsHistDialogContent[] _contents;

        HistContentsPager _pager;
        bool _isFirstOpen = true;
        int _prevRecordsLength;
        HistManager _manager;
        Action _closeAction;

        public void Initialize(HistManager manager)
        {
            if (!_isFirstOpen
                && _manager != manager)
            {
                _manager.OnRestored -= OnRestored;
                _manager.OnEmptyTrash -= OnEmptyTrash;
                _isFirstOpen = true;
            }
            _manager = manager;
            _manager.OnRestored += OnRestored;
            _manager.OnEmptyTrash += OnEmptyTrash;
        }

        void Start()
        {
            foreach (var content in _contents)
            {
                content.OnClicked += OnClickContent;
            }
            _closeButton.onClick.AddListener(Close);
            _nextButton.onClick.AddListener(OnNextButtonClicked);
            _prevButton.onClick.AddListener(OnPrevButtonClicked);
            _emptyTrashButton.onClick.AddListener(OnEmptyTrashButtonClicked);
        }

        void OnDestroy()
        {
            if (_manager == null)
            {
                return;
            }
            _manager.OnRestored -= OnRestored;
            _manager.OnEmptyTrash -= OnEmptyTrash;
        }

        public void Open(Action closeAction = null)
        {
            _closeAction = closeAction;
            OpenDialog();
        }

        protected override void OnOpenInternal()
        {
            if (!_isFirstOpen
                && _manager.Records.TrashLength == _prevRecordsLength)
            {
                return;
            }
            _isFirstOpen = false;
            _prevRecordsLength = _manager.Records.TrashLength;
            Refresh(true);
        }

        protected override void OnCloseInternal(bool isAwake)
        {
            if (isAwake)
            {
                return;
            }
            _closeAction?.Invoke();
            _closeAction = null;
        }

        void SetUp()
        {
            var records = _manager.Records;
            _pager = new HistContentsPager(records.TrashLength, _contents.Length);
        }

        void LoadContents()
        {
            _pagerText.text = _pager.Length > 0
                ? $"{_pager.StartIndex + 1}-{_pager.EndIndex + 1} / {_pager.Length}"
                : "---";
            var records = _manager.Records;
            var index = 0;
            if (_pager.Length > 0)
            {
                for (var i = _pager.StartIndex; i <= _pager.EndIndex; i++)
                {
                    var content = _contents[index];
                    content.gameObject.SetActive(true);
                    var record = records.GetFromTrashReverseAt(i);
                    content.Set(record);
                    index++;
                }
            }
            for (; index < _contents.Length; index++)
            {
                var content = _contents[index];
                content.gameObject.SetActive(false);
            }

            _nextButton.gameObject.SetActive(_pager.HasNext);
            _prevButton.gameObject.SetActive(_pager.HasPrev);
        }

        void Refresh(bool resetPage = false)
        {
            if (resetPage)
            {
                SetUp();
            }
            else
            {
                var length = _manager.Records.TrashLength;
                _pager.UpdateLength(length);
            }
            LoadContents();
        }

        void OnClickContent(HistRecord record)
        {
            _manager.RestoreFromTrash(record);
            _manager.Save();
        }

        void OnNextButtonClicked()
        {
            if (_pager.HasNext)
            {
                _pager.Next();
                LoadContents();
            }
        }

        void OnPrevButtonClicked()
        {
            if (_pager.HasPrev)
            {
                _pager.Prev();
                LoadContents();
            }
        }

        void OnEmptyTrashButtonClicked()
        {
            _manager.EmptyTrash();
            _manager.Save();
        }

        void OnRestored(HistRecord record) => Refresh();

        void OnEmptyTrash() => Refresh(true);
    }
}
