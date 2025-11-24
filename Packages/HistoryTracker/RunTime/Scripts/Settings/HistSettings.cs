
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HistoryTracker
{
    public sealed class HistSettings : ScriptableObject
    {
        public const string SettingFullPath = "Assets/Resources/HistoryTracker/Settings.asset";
        public const string SettingResourcesPath = "HistoryTracker/Settings";
        public const string AssetDstRootDir = "StreamingAssets/HistoryTracker";
        public const string AssetLoadRootDir = "HistoryTracker";

        [SerializeField, Tooltip("Specifies the activation scope for HistoryTracker")]
        ActivationScope _activationScope = ActivationScope.DevelopmentBuild;
        [SerializeField, Tooltip("Whether to include records generated on Editor in the build")]
        bool _includeRecordsInBuild = true;

        public ActivationScope CurrentScope => _activationScope;
        public bool IncludeRecordsInBuild => _includeRecordsInBuild;

        public enum ActivationScope
        {
            /// <summary>Only active in Editor</summary>
            EditorOnly = 0,
            /// <summary>Active in Editor and Development Build</summary>
            DevelopmentBuild = 1,
            /// <summary>Active in all builds (including Release)</summary>
            All = 2
        }

        public static HistSettings Current
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = CreateOrLoad();
                }
                return s_instance;
            }
        }
        static HistSettings s_instance;

        public static bool HasSettings => Load() != null;

        public bool IsScopeActive
        {
            get
            {
#if UNITY_EDITOR
                return true;
#elif DEVELOPMENT_BUILD
                return Current.CurrentScope == ActivationScope.DevelopmentBuild || Current.CurrentScope == ActivationScope.All;
#else
                return Current.CurrentScope == ActivationScope.All;
#endif
            }
        }

        public static HistSettings CreateOrLoad() => Load() ?? Create();

        public static HistSettings Create()
        {
#if UNITY_EDITOR
            var settings = ScriptableObject.CreateInstance<HistSettings>();
            var dir = Path.GetDirectoryName(SettingFullPath);
            if (dir != null && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            AssetDatabase.CreateAsset(settings, SettingFullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#else
            throw new System.InvalidOperationException("[HistoryTracker] Cannot create settings asset in editor.");
#endif
            return Load();
        }

        public static HistSettings Load() => Resources.Load<HistSettings>(SettingResourcesPath);
    }
}
