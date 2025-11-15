
using System;
using UnityEngine;
using UnityEngine.UI;

namespace HistoryTracker
{
    internal sealed class HistDialogContent : MonoBehaviour
    {
        public event Action<HistRecord> OnClicked = delegate {};
        
        [SerializeField] Text _titleText;
        [SerializeField] Button _openButton;
        
        public HistRecord Record { get; private set; }
        
        void Start() => _openButton.onClick.AddListener(OnOpenButtonClicked);

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