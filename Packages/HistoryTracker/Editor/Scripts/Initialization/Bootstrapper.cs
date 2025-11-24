
using UnityEditor;

namespace HistoryTracker.Editor
{
    internal sealed class Bootstrapper
    {
        const string _key = "HistoryTracker_Bootstrapper_IsInitialized";

        static bool IsInitialized
        {
            get => SessionState.GetBool(_key, false);
            set => SessionState.SetBool(_key, value);
        }

        [InitializeOnLoadMethod]
        static void OnDomainReload()
        {
            if (IsInitialized)
            {
                return;
            }
            IsInitialized = true;
            EditorApplication.delayCall += LoadSettings;
        }

        static void LoadSettings() => HistSettings.CreateOrLoad();
    }
}
