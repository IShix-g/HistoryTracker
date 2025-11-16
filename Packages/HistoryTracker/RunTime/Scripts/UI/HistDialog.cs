
using System;
using UnityEngine;

namespace HistoryTracker
{
    public abstract class HistDialog : MonoBehaviour
    {
        public event Action OnOpened = delegate {};
        public event Action OnClosed = delegate {};
        
        [SerializeField] RectTransform _parent;
        
        public bool IsOpen { get; private set; }

        protected abstract void OnOpenInternal();
        
        protected abstract void OnCloseInternal(bool isAwake);

        protected virtual void Awake()
        {
            _parent.gameObject.SetActive(false);
            OnCloseInternal(true);
        }
        
        protected void OpenDialog()
        {
            if (IsOpen)
            {
                return;
            }
            IsOpen = true;
            _parent.gameObject.SetActive(true);
            OnOpenInternal();
            OnOpened();
        }
        
        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }
            IsOpen = false;
            _parent.gameObject.SetActive(false);
            OnCloseInternal(false);
            OnClosed();
        }
    }
}