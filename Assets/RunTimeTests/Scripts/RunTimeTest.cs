#if UNITY_EDITOR
using System.IO;
using UnityEngine;

namespace Tests
{
    public class RunTimeTest : MonoBehaviour
    {
        readonly DummyData _data = new();

        void Awake() => _data.Create();

        void OnDestroy() => _data.Release();
    }
}
#endif