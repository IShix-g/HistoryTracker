
using System;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HistoryTracker
{
    /// <summary>
    /// Saves game data when an error occurs
    /// </summary>
    public sealed class HistErrorSaver : MonoBehaviour
    {
        /// <summary>
        /// Waiting time in seconds after an error occurs before saving (debounce).
        /// </summary>
        const float _defaultSaveDelayDuration = 10f;
        /// <summary>
        /// Minimum interval in seconds between save attempts.
        /// </summary>
        const float _defaultSaveCooldownDuration = 10f;

        static HistErrorSaver s_instance;

        readonly StringBuilder _pendingErrorLogs = new ();
        float _saveDelayDuration = _defaultSaveDelayDuration;
        float _saveCooldownDuration = _defaultSaveCooldownDuration;
        float _saveDelayTimer;
        float _cooldownTimer;

        public static void Create(float saveDelayDuration = 0, float saveCooldownDuration = 0)
        {
            if (!HistSettings.Current.IsScopeActive
                || s_instance != null)
            {
                return;
            }
            var go = new GameObject().AddComponent<HistErrorSaver>();
            go.SetDuration(saveDelayDuration, saveCooldownDuration);
        }

        public void SetDuration(float saveDelayDuration, float saveCooldownDuration)
        {
            if (saveDelayDuration > 0)
            {
                _saveDelayDuration = saveDelayDuration;
            }
            if (saveCooldownDuration > 0)
            {
                _saveCooldownDuration = saveCooldownDuration;
            }
        }

        void Awake()
        {
            if (!HistSettings.Current.IsScopeActive)
            {
                Destroy(this);
                return;
            }
            if (s_instance == null)
            {
                s_instance = this;
                DontDestroyOnLoad(gameObject);
#if UNITY_EDITOR
                var scripts = GetComponentsInChildren<MonoBehaviour>();
                if (scripts.Length > 1)
                {
                    Debug.LogError($"This object has {scripts.Length} scripts attached. {nameof(HistErrorSaver)} must be DontDestroyOnLoad, but are the other scripts acceptable? If not, please generate it using `ErrorHistorySaver.Create()` instead of attaching it in the Hierarchy.");
                }
#endif
            }
            else if (s_instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Application.logMessageReceivedThreaded += OnLogMessageReceived;
        }

        void OnDestroy() => Application.logMessageReceivedThreaded -= OnLogMessageReceived;

        void Update()
        {
            if (_cooldownTimer > 0)
            {
                _cooldownTimer -= Time.deltaTime;
                if (_cooldownTimer > 0)
                {
                    return;
                }
            }

            if (_saveDelayTimer > 0)
            {
                _saveDelayTimer -= Time.deltaTime;
                if (_saveDelayTimer <= 0)
                {
                    SaveHistory();
                }
            }
        }

        void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type is not (LogType.Exception or LogType.Error or LogType.Assert))
            {
                return;
            }

            _pendingErrorLogs.Append('[').Append(DateTime.Now.ToString("yyyy.MM.dd HH:mm")).Append("] Scene : ").Append(SceneManager.GetActiveScene().name).Append('\n');
            _pendingErrorLogs.Append('[').Append(type).Append("] ").Append(condition).Append('\n');
            _pendingErrorLogs.Append(stackTrace).Append('\n');

            _saveDelayTimer = _saveDelayDuration;
        }

        void SaveHistory()
        {
            var title = "[Error]";
            var description = _pendingErrorLogs.ToString();
            var info = new HistRecordInfo(title, description);
            Hist.SaveHistory(info);

            _pendingErrorLogs.Length = 0;
            _cooldownTimer = _saveCooldownDuration;
        }
    }
}
