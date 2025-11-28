#if DEBUG
using UnityEngine;
using UnityEngine.UI;
using HistoryTracker;
using UnityEngine.SceneManagement;

namespace Tests
{
    public class RunTimeTest : MonoBehaviour
    {
        [SerializeField] Button _dialogButton;
        [SerializeField] Button _saveButton;
        [SerializeField] Button _errorButton;
        [SerializeField] Button _reloadButton;

        TestModelRepository _repository;

        public void Inject(TestModelRepository repository) => _repository = repository;

        // Initialize the repository that implements IHistSaveDataHandler in Awake
        void Awake() => Hist.Configure(_repository);

        void Start()
        {
            HistErrorSaver.Create();
            _dialogButton.onClick.AddListener(OnDialogButtonClicked);
            _saveButton.onClick.AddListener(OnSaveButtonClicked);
            _errorButton.onClick.AddListener(OnErrorButtonClicked);
            _reloadButton.onClick.AddListener(OnReloadButtonClicked);
        }

        void OnDialogButtonClicked() => Hist.OpenDialog();

        void OnSaveButtonClicked()
        {
            var info = new HistRecordInfo("[Scripts]","[Scripts]");
            Hist.SaveHistory(info);
        }

        void OnErrorButtonClicked() => throw new System.Exception("Test Exception");

        void OnReloadButtonClicked() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        public sealed class User
        {
            public int Level { get; set; }
            public int Hp { get; set; }
            public int Mp { get; set; }
            public int Gold { get; set; }
        }
    }
}
#endif
