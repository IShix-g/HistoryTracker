#if DEBUG
using HistoryTracker;
using UnityEngine;
using UnityEngine.UI;

namespace Tests
{
    [RequireComponent(typeof(Button))]
    public sealed class HistDialogButton : MonoBehaviour
    {
        [SerializeField] Button _openButton;
        [SerializeField] HistDialog _dialog;

        void Start()
        {
            _openButton.onClick.AddListener(ClickDialog);
        }

        void ClickDialog() => _dialog.Open();

        void Reset()
        {
            _openButton = GetComponent<Button>();
            _dialog = FindFirstObjectByType<HistDialog>();
        }
    }
}
#endif