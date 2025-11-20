
using UnityEditor;
using UnityEngine;

namespace HistoryTracker.Editor
{
    internal sealed class RecordPopUpHistDialogButton
    {
        [MenuItem("Window/HistoryTracker/open Records dialog (Play mode only)")]
        public static void ShowDialog()
        {
            if (Application.isPlaying)
            {
                var obj = Hist.CreateOrGet();
                obj.OpenDialog(Hist.Release);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "History Tracker",
                    "The Records dialog can only be opened in Play mode.",
                    "OK"
                );
            }
        }
    }
}