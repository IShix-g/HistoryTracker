#if DEBUG
using System.IO;
using UnityEngine;

namespace Tests
{
    [DefaultExecutionOrder(-100)]
    public sealed class TestModelLoader : MonoBehaviour
    {
        [SerializeField] RunTimeTest _test;
        [SerializeField] RunTimeTestRecordLog _log;

        public TestModelRepository Repository { get; private set; } = new ();

        void Awake()
        {
            var paths = new[]
            {
                Path.Combine(Application.persistentDataPath, "TestData", "test1.bytes"),
                Path.Combine(Application.persistentDataPath, "test2.bytes"),
                Path.Combine(Application.persistentDataPath, "test3.bytes")
            };
            Repository.Load(paths);
            _test.Inject(Repository);
            _log.Inject(Repository);
        }

        // void OnDestroy() => Repository.DeleteAll();
    }
}
#endif
