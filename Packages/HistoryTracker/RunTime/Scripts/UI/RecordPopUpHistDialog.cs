
using System;
using UnityEngine;
using UnityEngine.UI;

namespace HistoryTracker
{
    internal sealed class RecordPopUpHistDialog : HistDialog
    {
        public event Action<HistRecord> OnValueChanged = delegate {};
        public event Action<HistRecord> OnRestore = delegate {};
        public event Action<HistRecord> OnDelete = delegate {};

        [SerializeField] Button _restoreButton;
        [SerializeField] Button _deleteButton;
        [SerializeField] Button _closeButton;
        [SerializeField] Text _idText;
        [SerializeField] Text _titleText;
        [SerializeField] Text _descriptionText;
        [SerializeField] Text _dateText;
        [SerializeField] InputField _titleInput;
        [SerializeField] InputField _descriptionInput;
        [SerializeField] HistRecordObjectShower _titleShower;
        [SerializeField] HistRecordObjectShower _descriptionShower;
        [SerializeField] RectTransform _caution;
        [SerializeField] RectTransform _editorIcon;

        public HistRecord Record { get; private set; }

        void Start()
        {
            _restoreButton.onClick.AddListener(OnRestoreButtonClicked);
            _deleteButton.onClick.AddListener(OnDeleteButtonClicked);
            _closeButton.onClick.AddListener(Close);
            _titleInput.onEndEdit.AddListener(OnEndEditTitle);
            _descriptionInput.onEndEdit.AddListener(OnEndEditDescription);
            _titleShower.OnShow += TitleShowerOnShow;
            _descriptionShower.OnShow += DescriptionShowerOnShow;
        }

        public void Open(HistRecord record)
        {
            Record = record;
            OpenDialog();
        }

        protected override void OnOpenInternal()
        {
            _idText.text = "ID:" + Record.Id;
            _titleText.text = Record.Title;
            _descriptionText.text = Record.Description;
            _titleInput.text = Record.Title;
            _descriptionInput.text = Record.Description;
            var savedDate = DateTime.Parse(Record.TimeStamp);
            _dateText.text = savedDate.ToString("g", LocaleProvider.Culture);

            _titleInput.gameObject.SetActive(false);
            _descriptionInput.gameObject.SetActive(false);

#if UNITY_EDITOR
            _titleShower.enabled = true;
            _descriptionShower.enabled = true;
            _caution.gameObject.SetActive(false);
            _editorIcon.gameObject.SetActive(true);
#else
            _titleShower.enabled = !Record.IsStreamingAssets;
            _descriptionShower.enabled = !Record.IsStreamingAssets;
            _caution.gameObject.SetActive(Record.IsStreamingAssets);
            _editorIcon.gameObject.SetActive(Record.IsStreamingAssets);
#endif
        }

        protected override void OnCloseInternal(bool isAwake) {}

        void OnRestoreButtonClicked()
        {
            if (IsOpen)
            {
                OnRestore(Record);
                Close();
            }
        }

        void OnDeleteButtonClicked()
        {
            if (IsOpen)
            {
                OnDelete(Record);
                Close();
            }
        }

        void OnEndEditTitle(string text)
        {
            if (IsOpen
                && Record.Title != text)
            {
                Record.Title = text;
                _titleText.text = text;
                OnValueChanged(Record);
            }
            _titleShower.Hide();
        }

        void OnEndEditDescription(string text)
        {
            if (IsOpen && Record.Description != text)
            {
                Record.Description = text;
                _descriptionText.text = text;
                OnValueChanged(Record);
            }
            _descriptionShower.Hide();
        }

        void TitleShowerOnShow() => _descriptionShower.Hide();

        void DescriptionShowerOnShow() => _titleShower.Hide();
    }
}
