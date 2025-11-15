
using System;
using UnityEngine;
using UnityEngine.UI;

namespace HistoryTracker
{
    public sealed class HistDialog : HistDialogBase
    {
        [SerializeField] RectTransform _bg;
        [SerializeField] Button _saveButton;
        [SerializeField] Button _closeButton;
        [SerializeField] Button _nextButton;
        [SerializeField] Button _prevButton;
        [SerializeField] Text _pagerText;
        [SerializeField] HistRecordPopUp _recordPopUp;
        [SerializeField] HistDialogContent[] _contents;
        
        HistContentsPager _pager;
        bool _isFirstOpen = true;

        void Start()
        {
            if (Hist.Manager == null)
            {
                throw new InvalidOperationException("`Hist.Configure()` must be called before using HistoryTracker");
            }
            foreach (var content in _contents)
            {
                content.OnClicked += OnClickContent;
            }
            _recordPopUp.OnRestore += PopUpOnRestore;
            _recordPopUp.OnDelete += PopUpOnDelete;
            _recordPopUp.OnValueChanged += PopUpOnValueChanged;
            Hist.Manager.OnAddRecord += OnAddRecord;
            Hist.Manager.OnRemoveRecord += OnRemoveRecord;
            _saveButton.onClick.AddListener(OnSaveButtonClicked);
            _closeButton.onClick.AddListener(Close);
            _nextButton.onClick.AddListener(OnNextButtonClicked);
            _prevButton.onClick.AddListener(OnPrevButtonClicked);
        }

        void OnDestroy()
        {
            if (Hist.Manager != null)
            {
                Hist.Manager.OnAddRecord -= OnAddRecord;
                Hist.Manager.OnRemoveRecord -= OnRemoveRecord;
                Hist.Manager.Save();
            }
        }

        public void Open() => OpenDialog();
        
        protected override void OnOpenInternal()
        {
            _bg.gameObject.SetActive(true);
            if (_recordPopUp.IsOpen)
            {
                _recordPopUp.Close();
            }

            if (_isFirstOpen)
            {
                _isFirstOpen = false;
                SetUp();
                LoadContents();
            }
        }

        protected override void OnCloseInternal(bool isAwake)
        {
            _bg.gameObject.SetActive(false);
        }

        void SetUp()
        {
            var records = Hist.Manager.Records;
            _pager = new HistContentsPager(records.Length, _contents.Length);
        }

        void LoadContents()
        {
            _pagerText.text = _pager.Length > 0
                ? $"{_pager.StartIndex + 1}-{_pager.EndIndex + 1} / {_pager.Length}"
                : "---";
            var records = Hist.Manager.Records;
            var index = 0;
            if (_pager.Length > 0)
            {
                for (var i = _pager.StartIndex; i <= _pager.EndIndex; i++)
                {
                    var content = _contents[index];
                    content.gameObject.SetActive(true);
                    var record = records.GetRevirseAt(i);
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
            var length = Hist.Manager.Records.Length;
            _pager.UpdateLength(length);
            LoadContents();
        }

        void OnSaveButtonClicked()
        {
            Hist.Manager.SaveHistory();
            Hist.Manager.Save();
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
            Hist.Manager.Apply(record);
            Close();
        }

        void PopUpOnDelete(HistRecord record)
        {
            Hist.Manager.Remove(record);
            Hist.Manager.Save();
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