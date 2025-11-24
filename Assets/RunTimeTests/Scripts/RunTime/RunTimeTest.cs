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
            Hist.OpenDialog();
        }

        void OnSaveButtonClicked()
        {
            var info = new HistRecordInfo("[Scripts]","[Scripts]");
            Hist.SaveHistory(info);
        }
    }
}
#endif
