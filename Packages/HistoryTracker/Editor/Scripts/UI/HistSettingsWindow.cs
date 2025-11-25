
using System.Threading;
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
        const string _logoPath = PackagePath + "Editor/Textures/historyTrackerLogo.png";

        [MenuItem("Window/HistoryTracker/open Settings", priority = 0)]
        static void ShowWindow()
        {
            var window = GetWindow<HistSettingsWindow>();
            window.titleContent = new GUIContent("HistoryTracker Settings");
            window.Show();
        }

        readonly PackageVersionChecker _versionChecker = new (_gitInstallUrl, _gitBranchName, _packageName);
        SerializedObject _settingsObject;
        Texture2D _logo;
        GUIContent _updateIcon;
        GUIContent _helpHeader;
        bool _hasNewVersion;
        readonly CancellationTokenSource _tokenSource = new ();

        void OnEnable()
        {
            _logo = AssetDatabase.LoadAssetAtPath<Texture2D>(_logoPath);
            _updateIcon = EditorGUIUtility.IconContent("CloudConnect");
            _helpHeader = EditorGUIUtility.IconContent("Help");
            var setting = HistSettings.Current;
            _settingsObject = new SerializedObject(setting);
            _versionChecker.Fetch().Handled(_ =>
            {
                _hasNewVersion = _versionChecker.HasNewVersion();
            });
        }

        void OnDestroy()
        {
            _logo = null;
            _updateIcon = null;
            _helpHeader = null;
            _settingsObject.Dispose();
            _versionChecker?.Dispose();
            _tokenSource?.SafeCancelAndDispose();
        }

        void OnGUI()
        {
            {
                var style = new GUIStyle()
                {
                    padding = new RectOffset(5, 5, 5, 5),
                };
                GUILayout.BeginVertical(style);
            }

            EditorGUI.BeginDisabledGroup(_versionChecker.IsProcessing);
            {
                var style = new GUIStyle()
                {
                    padding = new RectOffset(5, 5, 5, 5),
                };
                GUILayout.BeginHorizontal(style);
            }
            var width = GUILayout.Width(33);
            var height = GUILayout.Height(EditorGUIUtility.singleLineHeight + 5);
            var clickedGitHub = GUILayout.Button(_helpHeader, width, height);
            var clickedDialog = GUILayout.Button("open History dialog (Play mode only)", height);
            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            if (clickedGitHub)
            {
                Application.OpenURL(_gitURL);
            }
            else if (clickedDialog)
            {
                HistoryPopUpHistDialogMenu.ShowDialog();
            }

            if(_versionChecker.IsLoaded
               && !_versionChecker.IsProcessing
               && _hasNewVersion)
            {
                var style = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(10, 10, 10, 10),
                    margin = new RectOffset(0, 0, 0, 5)
                };
                var textStyle = new GUIStyle(GUI.skin.label)
                {
                    richText = true,
                    fontSize = 14,
                    normal =
                    {
                        textColor = EditorGUIUtility.isProSkin ? Color.yellow : Color.blue
                    }
                };

                GUILayout.BeginHorizontal(style);
                GUILayout.Label(_updateIcon, GUILayout.Width(20), GUILayout.Height(20));
                var version = "<b>" + _versionChecker.ServerInfo.VersionString + "</b> is now available";
                GUILayout.Label(version, textStyle);
                var clickedVersion = GUILayout.Button("Update", GUILayout.Width(80));
                GUILayout.EndHorizontal();
                if(clickedVersion)
                {
                    _versionChecker.Install(_tokenSource.Token);
                    _hasNewVersion = false;
                }
            }

            {
                var style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    margin = new RectOffset(0, 0, 0, 5)
                };
                GUILayout.Label(_logo, style, GUILayout.MinWidth(400), GUILayout.Height(60.5f));
            }

            _settingsObject.Update();
            var property = _settingsObject.GetIterator();
            property.NextVisible(true);
            while (property.NextVisible(false))
            {
                EditorGUILayout.PropertyField(property, true);
            }
            _settingsObject.ApplyModifiedProperties();

            {
                var style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleRight,
                    margin = new RectOffset(0, 0, 5, 0)
                };
                var pluginVersion = _versionChecker.IsLoaded ? _versionChecker.LocalInfo.VersionString : "---";
                GUILayout.Label(pluginVersion, style);
            }

            GUILayout.EndVertical();
        }
    }
}
