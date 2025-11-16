
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HistoryTracker
{
    internal sealed class HistRecordObjectShower : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public event Action OnShow = delegate {};
        public event Action OnHide = delegate {};
        
        [SerializeField] RectTransform _parent;
        [SerializeField] bool _hideOnAwake = true;

        public bool IsShow { get; private set; }

        float _tapTime;

        void Awake()
        {
            if (_hideOnAwake)
            {
                Hide();
            }
        }

        void Update()
        {
            if (_tapTime <= 0)
            {
                return;
            }
            _tapTime -= Time.deltaTime;

            if (_tapTime <= 0)
            {
                if (IsShow)
                {
                    Hide();
                }
                else
                {
                    Show();
                }
            }
        }

        public void Show()
        {
            IsShow = true;
            _parent.gameObject.SetActive(true);
            OnShow();
        }
        
        public void Hide()
        {
            IsShow = false;
            _parent.gameObject.SetActive(false);
            OnHide();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _tapTime = 1.5f;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _tapTime = 0;
        }
    }
}