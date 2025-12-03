
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace HistoryTracker.Editor
{
    internal sealed class HistSettingsWindow : EditorWindow
    {
        public const string PackagePath = "Packages/" + _packageName + "/";
        const string _gitURL = "https://github.com/IShix-g/HistoryTracker";
        const string _gitInstallUrl = _gitURL + ".git?path=Packages/HistoryTracker";
        const string _gitBranchName = "main";
        const string _packageName = "com.ishix.history.tracker";
        const string _logoPath = PackagePath + "Editor/Textures/historyTrackerLogo.png";

        [MenuItem("Window/open History Tracker", priority = 0)]
        static void ShowWindow()
        {
            var window = GetWindow<HistSettingsWindow>();
            window.titleContent = new GUIContent("History Tracker");
            window.Show();
        }

        string Title
        {
            get => EditorPrefs.GetString("HistSettingsWindow_Title");
            set => EditorPrefs.SetString("HistSettingsWindow_Title", value);
        }
        string Description
        {
            get => EditorPrefs.GetString("HistSettingsWindow_Description");
            set => EditorPrefs.SetString("HistSettingsWindow_Description", value);
        }

        readonly PackageVersionChecker _versionChecker = new (_gitInstallUrl, _gitBranchName, _packageName);
        SerializedObject _settingsObject;
        Texture2D _logo;
        GUIContent _updateIcon;
        GUIContent _helpHeader;
        bool _hasNewVersion;
        bool _showSettings = true;
        string _title;
        string _description;
        readonly CancellationTokenSource _tokenSource = new ();

        void OnEnable()
        {
            _title = Title;
            _description = Description;
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
            Title = _title;
            Description = _description;
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
            _showSettings = EditorGUILayout.Foldout(_showSettings, "Build Setting", true);
            if (_showSettings)
            {
                var style = new GUIStyle()
                {
                    padding = new RectOffset(10, 10, 10, 10)
                };
                GUILayout.BeginVertical(style);
                var property = _settingsObject.GetIterator();
                property.NextVisible(true);
                while (property.NextVisible(false))
                {
                    EditorGUILayout.PropertyField(property, true);
                }
                GUILayout.EndVertical();
            }
            _settingsObject.ApplyModifiedProperties();

            {
                var wrapStyle = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(10, 10, 10, 10),
                    margin = new RectOffset(0, 0, 5, 5)
                };
                GUILayout.BeginVertical(wrapStyle);
                var textStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    margin = new RectOffset(0, 0, 0, 5)
                };
                GUILayout.Label("History Recorder\n(Play mode only)", textStyle);

                EditorGUILayout.HelpBox("(Optional) Add a title and description.", MessageType.Info);
                _title = EditorGUILayout.TextField("Title", _title);
                _description = EditorGUILayout.TextField("Description", _description);

                var buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    padding = new RectOffset(5, 5, 5, 5),
                    margin = new RectOffset(0, 0, 10, 0)
                };
                if (GUILayout.Button("Save", buttonStyle))
                {
                    if (Application.isPlaying)
                    {
                        var info = default(HistRecordInfo);
                        if (!string.IsNullOrEmpty(_title)
                            || !string.IsNullOrEmpty(_description))
                        {
                            info = new HistRecordInfo(_title, _description);
                        }
                        Hist.SaveHistory(info);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog(
                            "History Tracker",
                            "Can only be saved in Play mode.",
                            "OK"
                        );
                    }
                }
                GUILayout.EndVertical();
            }

            {
                var style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleRight
                };
                var pluginVersion = _versionChecker.IsLoaded ? _versionChecker.LocalInfo.VersionString : "---";
                GUILayout.Label(pluginVersion, style);
            }

            GUILayout.EndVertical();
        }
    }
}
