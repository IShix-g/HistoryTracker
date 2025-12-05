
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
        [SerializeField] Button _trashButton;
        [SerializeField] Text _pagerText;
        [SerializeField] RecordPopUpHistDialog _recordPopUp;
        [SerializeField] RecordsHistDialogContent[] _contents;
        [SerializeField] TrashRecordsHistDialog _trashDialog;

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
                UnbindManagerEvents();
                _isFirstOpen = true;
            }
            _manager = manager;
            BindManagerEvents();
            _trashDialog.Initialize(manager);
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
            _trashButton.onClick.AddListener(OnTrashButtonClicked);
        }

        void OnDestroy()
        {
            if (_manager != null)
            {
                UnbindManagerEvents();
            }
        }

        void BindManagerEvents()
        {
            _manager.OnStartSave += OnStartSave;
            _manager.OnEndSave += OnEndSave;
            _manager.OnStartAddRecord += OnStartAddRecord;
            _manager.OnEndAddRecord += OnEndAddRecord;
            _manager.OnAddRecord += OnAddRecord;
            _manager.OnRemoveRecord += OnRemoveRecord;
            _manager.OnStartApply += OnStartApply;
            _manager.OnEndApply += OnEndApply;
        }

        void UnbindManagerEvents()
        {
            _manager.OnStartSave -= OnStartSave;
            _manager.OnEndSave -= OnEndSave;
            _manager.OnStartAddRecord += OnStartAddRecord;
            _manager.OnEndAddRecord -= OnEndAddRecord;
            _manager.OnAddRecord -= OnAddRecord;
            _manager.OnRemoveRecord += OnRemoveRecord;
            _manager.OnStartApply -= OnStartApply;
            _manager.OnEndApply -= OnEndApply;
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

            UnLockDialog();

            if (!_isFirstOpen
                && _manager.Records.Length == _prevRecordsLength)
            {
                return;
            }
            _isFirstOpen = false;
            _prevRecordsLength = _manager.Records.Length;
            Refresh(true);
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
            if (_trashDialog.IsOpen)
            {
                _trashDialog.Close();
            }
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

        void Refresh(bool resetPage = false)
        {
            if (resetPage)
            {
                SetUp();
            }
            else
            {
                var length = _manager.Records.Length;
                _pager.UpdateLength(length);
            }
            LoadContents();
        }

        void OnClickContent(HistRecord record) => _recordPopUp.Open(record);

        void OnAddRecord(HistRecord record)
        {
            if (IsOpen)
            {
                Refresh(true);
            }
        }

        void OnRemoveRecord(HistRecord record)
        {
            if (IsOpen)
            {
                Refresh();
            }
        }

        void OnStartSave()
        {
            if (IsOpen)
            {
                LockDialog();
            }
        }

        void OnEndSave()
        {
            if (IsOpen)
            {
                UnLockDialog();
            }
        }

        void OnStartAddRecord()
        {
            if (IsOpen)
            {
                LockDialog();
            }
        }

        void OnEndAddRecord()
        {
            if (IsOpen)
            {
                UnLockDialog();
            }
        }

        void OnStartApply()
        {
            if (IsOpen)
            {
                LockDialog();
            }
        }

        void OnEndApply(HistAppliedInfo info)
        {
            if (IsOpen)
            {
                UnLockDialog();
            }
        }

        void OnSaveButtonClicked()
            => _manager.SaveHistory(null, HistRecordInfoPlacement.Prepend, _ => _manager.Save());

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

        void OnTrashButtonClicked()
        {
            if (!_trashDialog.IsOpen)
            {
                _trashDialog.Open();
            }
        }

        void PopUpOnRestore(HistRecord record)
        {
            _manager.Apply(record, isSuccess =>
            {
                if (isSuccess)
                {
                    Close();
                }
            });
        }

        void PopUpOnDelete(HistRecord record)
        {
            _manager.MoveToTrash(record);
            _manager.Save();
        }

        void PopUpOnValueChanged(HistRecord record)
        {
            foreach (var content in _contents)
            {
                if(content.Record == record)
                {
                    content.UpdateContent();
                    _manager.Save();
                    break;
                }
            }
        }

        void LockDialog()
        {
            _saveButton.interactable = false;
            _closeButton.interactable = false;
            _nextButton.interactable = false;
            _prevButton.interactable = false;
        }

        void UnLockDialog()
        {
            _saveButton.interactable = true;
            _closeButton.interactable = true;
            _nextButton.interactable = true;
            _prevButton.interactable = true;
        }
    }
}
