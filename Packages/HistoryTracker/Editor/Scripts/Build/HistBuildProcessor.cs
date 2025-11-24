
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEditor;

namespace HistoryTracker.Editor
{
    internal sealed class HistBuildProcessor :
        IPreprocessBuildWithReport,
        IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        bool _isCopiedResources;
        
        public void OnPreprocessBuild(BuildReport report)
        {
            if (!HistSettings.HasSettings)
            {
                return;
            }
            if (IsActiveHistoryTracker())
            {
                EditorApplication.update += OnUpdate;
                _isCopiedResources = true;
                WriteResource(HistUI.ResourcesPathForEditor, HistUI.ResourcesFullPath);
            }
            if (HistSettings.Current.IncludeRecordsInBuild)
            {
                if (!_isCopiedResources)
                {
                    EditorApplication.update += OnUpdate;
                }
                _isCopiedResources = true;
                var resourceDir = Path.GetDirectoryName(EditorHistDataService.GetDataPath(Hist.DirectoryName));
                var dstDir = Path.Combine(Application.dataPath, HistSettings.AssetDstRootDir);
                CopyResourceDirectory(resourceDir, dstDir);
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            EditorApplication.update -= OnUpdate;
            if (_isCopiedResources)
            {
                _isCopiedResources = false;
                Cleanup();
            }
        }

        void OnUpdate()
        {
            if (!BuildPipeline.isBuildingPlayer)
            {
                OnPostprocessBuild(default);
            }
        }
        
        void Cleanup()
        {
            if (!HistSettings.HasSettings)
            {
                return;
            }
            if (IsActiveHistoryTracker())
            {
                DeleteResource(HistUI.ResourcesFullPath);
            }
            if (HistSettings.Current.IncludeRecordsInBuild)
            {
                var dstDir = Path.Combine(Application.dataPath, HistSettings.AssetDstRootDir);
                DeleteResourceDirectory(dstDir);
            }
        }
        
        bool IsActiveHistoryTracker()
        {
            var scope = HistSettings.Current.CurrentScope;
            return (scope == HistSettings.ActivationScope.DevelopmentBuild && EditorUserBuildSettings.development)
                    || scope == HistSettings.ActivationScope.All;
        }

        void WriteResource(string resourcePath, string dstPath)
        {
            var dstDir = Path.GetDirectoryName(dstPath);
            if (!string.IsNullOrEmpty(dstDir)
                && !Directory.Exists(dstDir))
            {
                Directory.CreateDirectory(dstDir);
            }
            AssetDatabase.CopyAsset(resourcePath, dstPath);
            Debug.Log("[HistoryTracker] Resource file created at " + dstPath + ".");
        }

        void DeleteResource(string dstPath)
        {
            if (File.Exists(dstPath))
            {
                AssetDatabase.DeleteAsset(dstPath);
            }
        }

        void CopyResourceDirectory(string resourceRootDir, string dstRootDir)
        {
            if (!Directory.Exists(resourceRootDir))
            {
                Debug.LogWarning($"[HistoryTracker] Source directory not found: {resourceRootDir}");
                return;
            }
            if (!Directory.Exists(dstRootDir))
            {
                Directory.CreateDirectory(dstRootDir);
            }
            foreach (var dir in Directory.GetDirectories(resourceRootDir, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(resourceRootDir, dir);
                var dstDir = Path.Combine(dstRootDir, relative);
                if (!Directory.Exists(dstDir))
                {
                    Directory.CreateDirectory(dstDir);
                }
            }
            foreach (var file in Directory.GetFiles(resourceRootDir, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(resourceRootDir, file);
                var dstFile = Path.Combine(dstRootDir, relative);
                var dstDir = Path.GetDirectoryName(dstFile);
                if (!string.IsNullOrEmpty(dstDir) && !Directory.Exists(dstDir))
                {
                    Directory.CreateDirectory(dstDir);
                }
                File.Copy(file, dstFile, overwrite: true);
            }
            AssetDatabase.Refresh();
            Debug.Log("[HistoryTracker] Resource directory copied at " + dstRootDir + ".");
        }

        void DeleteResourceDirectory(string dstRootDir)
        {
            if (Directory.Exists(dstRootDir))
            {
                DeleteDirectory(dstRootDir, true);
            }
            var dir = Path.GetDirectoryName(dstRootDir);
            DeleteEmptyDirectoryUpwards(dir);
            AssetDatabase.Refresh();
        }
        
        void DeleteEmptyDirectoryUpwards(string dirPath)
        {
            dirPath = dirPath.Replace("\\", "/");

            while (true)
            {
                if (!Directory.Exists(dirPath))
                {
                    return;
                }
                var dirName = Path.GetFileName(dirPath);
                if (dirName == "Assets"
                    || !dirPath.Contains("Assets"))
                {
                    return;
                }
                
                var fileSystemEntries = Directory.GetFileSystemEntries(dirPath);

                if (fileSystemEntries.Length == 0)
                {
                    DeleteDirectory(dirPath);
                    dirPath = Directory.GetParent(dirPath)?.FullName.Replace("\\", "/");
                    if (string.IsNullOrEmpty(dirPath))
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }
        
        void DeleteDirectory(string path, bool recursive = false)
        {
            Directory.Delete(path, recursive);
            var metaPath = path + ".meta";
            if (File.Exists(metaPath))
            {
                File.Delete(metaPath);
            }
        }
    }
}