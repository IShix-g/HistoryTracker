#if DEBUG
using UnityEngine;
using UnityEngine.UI;

namespace Tests
{
    public sealed class RunTimeTestRecordLog : MonoBehaviour
    {
        [SerializeField] Text _text;
        
        TestModelRepository _repository;
        
        public void Inject(TestModelRepository repository) => _repository = repository;
        
        void Start()
        {
            _repository.OnRestored += OnRestored;
            OnRestored();
        }
        
        void OnRestored()
        {
            var texts = string.Empty;
            foreach (var model in _repository.Models)
            {
                texts += $"Saved Count: {model.SaveCount}\n";
            }
            _text.text = texts;
        }
    }
}
#endif