
using System;
using UnityEngine;
using UnityEngine.UI;

namespace HistoryTracker
{
    internal sealed class RecordsHistDialog : HistDialog
    {
        [SerializeField] RectTransform _bg;
        [SerializeField] Button _saveButton;
        [SerializeField] Button _closeButton;
        [SerializeField] Button _nextButton;
        [SerializeField] Button _prevButton;
        [SerializeField] Text _pagerText;
        [SerializeField] RecordPopUpHistDialog _recordPopUp;
        [SerializeField] RecordsHistDialogContent[] _contents;
        
        HistContentsPager _pager;
        bool _isFirstOpen = true;
        HistManager _manager;
        Action _closeAction;

        public void Set(HistManager manager)
        {
            if (!_isFirstOpen
                && _manager != manager)
            {
                _manager.OnAddRecord -= OnAddRecord;
                _manager.OnRemoveRecord -= OnRemoveRecord;
                _isFirstOpen = true;
            }
            _manager = manager;
            _manager.OnAddRecord += OnAddRecord;
            _manager.OnRemoveRecord += OnRemoveRecord;
        }
        
        void Start()
        {
            foreach (var content in _contents)
            {
                content.OnClicked += OnClickContent;
            }
            _recordPopUp.OnRestore += PopUpOnRestore;
            _recordPopUp.OnDelete += PopUpOnDelete;
            _recordPopUp.OnValueChanged += PopUpOnValueChanged;
            _saveButton.onClick.AddListener(OnSaveButtonClicked);
            _closeButton.onClick.AddListener(Close);
            _nextButton.onClick.AddListener(OnNextButtonClicked);
            _prevButton.onClick.AddListener(OnPrevButtonClicked);
        }

        void OnDestroy()
        {
            if (_manager != null)
            {
                _manager.OnAddRecord -= OnAddRecord;
                _manager.OnRemoveRecord -= OnRemoveRecord;
                _manager.Save();
            }
        }

        public void Open(Action closeAction = null)
        {
            _closeAction = closeAction;
            OpenDialog();
        }
        
        protected override void OnOpenInternal()
        {
            _bg.gameObject.SetActive(true);
            if (_recordPopUp.IsOpen)
            {
                _recordPopUp.Close();
            }

            if (!_isFirstOpen)
            {
                return;
            }
            _isFirstOpen = false;
            SetUp();
            LoadContents();
        }

        protected override void OnCloseInternal(bool isAwake)
        {
            _bg.gameObject.SetActive(false);
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
            _pager = new HistContentsPager(records.Length, _contents.Length);
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
                    var record = records.GetReverseAt(i);
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
        
        void OnClickContent(HistRecord record) => _recordPopUp.Open(record);

        void OnAddRecord(HistRecord record)
        {
            SetUp();
            LoadContents();
        }

        void OnRemoveRecord(HistRecord record)
        {
            var length = _manager.Records.Length;
            _pager.UpdateLength(length);
            LoadContents();
        }

        void OnSaveButtonClicked()
        {
            _manager.SaveHistory();
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
        
        void PopUpOnRestore(HistRecord record)
        {
            _manager.Apply(record);
            Close();
        }

        void PopUpOnDelete(HistRecord record)
        {
            _manager.Remove(record);
            _manager.Save();
        }

        void PopUpOnValueChanged(HistRecord record)
        {
            foreach (var content in _contents)
            {
                if(content.Record == record)
                {
                    content.UpdateContent();
                    break;
                }
            }
        }
    }
}