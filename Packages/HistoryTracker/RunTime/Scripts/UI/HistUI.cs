
using System;
using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HistoryTracker
{
    public sealed class HistUI : MonoBehaviour
    {
        internal const string ResourcesPathForEditor = "Packages/com.ishix.history.tracker/RunTime/Prefabs/HistoryTracker.prefab";
        internal const string ResourcesPath = "HistoryTracker/HistoryTracker";
        internal static readonly string ResourcesFullPath = $"Assets/Resources/{ResourcesPath}.prefab";

        [SerializeField] RecordsHistDialog _dialog;

        static HistUI s_instance;

        GameObject _eventSystem;

        internal static HistUI CreateOrGet(HistManager manager)
        {
            if (s_instance != null)
            {
                s_instance._dialog.Initialize(manager);
                return s_instance;
            }
#if UNITY_EDITOR
            var prefab = AssetDatabase.LoadAssetAtPath<HistUI>(ResourcesPathForEditor);
#else
            var prefab = Resources.Load<HistUI>(ResourcesPath);
#endif
            var go = Instantiate(prefab);
            s_instance = go;
            s_instance._dialog.Initialize(manager);
            s_instance.Initialize();
            return go;
        }

        void Initialize()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            _eventSystem = new GameObject("EventSystem");
            _eventSystem.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            _eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            _eventSystem.AddComponent<StandaloneInputModule>();
#endif
        }

        internal static void Release()
        {
            if (s_instance._eventSystem != null)
            {
                Destroy(s_instance._eventSystem);
            }
            if (s_instance != null)
            {
                Destroy(s_instance.gameObject);
            }
        }

        public void OpenDialog(Action closeAction = null) => _dialog.Open(closeAction);

        public void CloseDialog() => _dialog.Close();
    }
}
