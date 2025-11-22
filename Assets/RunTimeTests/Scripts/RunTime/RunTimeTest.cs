#if DEBUG
using UnityEngine;
using UnityEngine.UI;
using HistoryTracker;

namespace Tests
{
    public class RunTimeTest : MonoBehaviour
    {
        [SerializeField] Button _dialogButton;
        [SerializeField] Button _saveButton;

        TestModelRepository _repository;

        public void Inject(TestModelRepository repository) => _repository = repository;

        // Initialize the repository that implements IHistSaveDataHandler in Awake
        void Awake() => Hist.Configure(_repository);

        void Start()
        {
            _dialogButton.onClick.AddListener(OnDialogButtonClicked);
            _saveButton.onClick.AddListener(OnSaveButtonClicked);
        }

        void OnDialogButtonClicked()
        {
            // Display a dialog by creating an object and calling OpenDialog.
            var obj = Hist.CreateOrGetUI();
            obj.OpenDialog(() =>
            {
                // You can also release it when the dialog closes.
                // Hist.Release();
            });
        }

        void OnSaveButtonClicked() => Hist.SaveHistory();
    }
}
#endif
