
using UnityEditor;
using UnityEngine;

namespace HistoryTracker.Editor
{
    internal class HistSettingsWindow : EditorWindow
    {
        public const string PackagePath = "Packages/" + _packageName + "/";
        const string _gitURL = "https://github.com/IShix-g/HistoryTracker";
        const string _gitInstallUrl = _gitURL + ".git?path=Packages/HistoryTracker";
        const string _gitBranchName = "main";
        const string _packageName = "com.ishix.history.tracker";
        const string _logoPath = PackagePath + "Editor/historyTrackerLogo.png";
        
        [MenuItem("Window/HistoryTracker/open Settings")]
        static void ShowWindow()
        {
            var window = GetWindow<HistSettingsWindow>();
            window.titleContent = new GUIContent("HistoryTracker Settings");
            window.Show();
        }

        SerializedObject _settingsObject;
        Texture2D _logo;

        void OnEnable()
        {
            _logo = AssetDatabase.LoadAssetAtPath<Texture2D>(_logoPath);
            var setting = HistSettings.Current;
            _settingsObject = new SerializedObject(setting);
        }

        void OnDestroy()
        {
            _logo = null;
        }
        
        void OnGUI()
        {
            {
                var style = new GUIStyle(GUI.skin.label)
                {
                    padding = new RectOffset(10, 10, 10, 10),
                };
                GUILayout.BeginVertical(style);
            }
            {
                var style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                };
                GUILayout.Label(_logo, style, GUILayout.MinWidth(400), GUILayout.Height(60.5f));
                GUILayout.Space(10);
            }
            
            _settingsObject.Update();
            var property = _settingsObject.GetIterator();
            property.NextVisible(true);
            while (property.NextVisible(false))
            {
                EditorGUILayout.PropertyField(property, true);
            }
            _settingsObject.ApplyModifiedProperties();
            
            GUILayout.EndVertical();
        }
    }
}