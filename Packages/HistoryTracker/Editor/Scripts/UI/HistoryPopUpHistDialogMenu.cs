
using UnityEditor;
using UnityEngine;

namespace HistoryTracker.Editor
{
    internal sealed class HistoryPopUpHistDialogMenu
    {
        public static void ShowDialog()
        {
            if (Application.isPlaying)
            {
                var obj = Hist.CreateOrGetUI();
                obj.OpenDialog(Hist.Release);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "History Tracker",
                    "The History dialog can only be opened in Play mode.",
                    "OK"
                );
            }
        }
    }
}
