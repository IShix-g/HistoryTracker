
using System;
using UnityEngine;
using UnityEngine.UI;

namespace HistoryTracker
{
    internal sealed class TrashRecordsHistDialogContent : MonoBehaviour
    {
        public event Action<HistRecord> OnClicked = delegate {};

        [SerializeField] Text _titleText;
        [SerializeField] Button _restoreButton;

        public HistRecord Record { get; private set; }

        void Start() => _restoreButton.onClick.AddListener(OnOpenButtonClicked);

        public void Set(HistRecord record)
        {
            Record = record;
            UpdateContent();
        }

        public void UpdateContent()
            => _titleText.text = Record.Title;

        void OnOpenButtonClicked() => OnClicked(Record);
    }
}
