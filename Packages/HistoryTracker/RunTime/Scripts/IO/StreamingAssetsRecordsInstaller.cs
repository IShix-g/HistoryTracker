
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Pool;

namespace HistoryTracker
{
    internal sealed class StreamingAssetsRecordsInstaller
    {
        static readonly string s_sourceRecordsPath = Path.Combine(Application.streamingAssetsPath, HistSettings.AssetLoadRootDir, Hist.RecordsFileName);
        static readonly string s_sourceRootDir = Path.Combine(Application.streamingAssetsPath, HistSettings.AssetLoadRootDir);

        public static void Install(HistManager manager) => InstallAsync(manager).Handled();

        public static async Task InstallAsync(HistManager manager)
        {
            var recordsBytes = await LoadBytes(s_sourceRecordsPath);
            var records = FileBasedHistDataService.Load(recordsBytes);
            if (records == null)
            {
                return;
            }
            while (manager.Records.MachineId == records.MachineId)
            {
                manager.Records.SetNewMachineId();
            }
            foreach (var record in records)
            {
                record.IsStreamingAssets = true;
                manager.Records.AddOrSet(record);
            }
            manager.Save();
        }

        public static string GetFullPath(string relativePath) => Path.Combine(s_sourceRootDir, relativePath);

        public static async Task<byte[]> LoadBytes(string filePath)
        {
            var useWebRequest = Uri.IsWellFormedUriString(filePath, UriKind.Absolute);
            if (useWebRequest)
            {
                using var request = UnityWebRequest.Get(filePath);
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.data;
                }
                Debug.LogError($"[HistoryTracker] LoadBytes failed: {request.error}\n{filePath}");
            }
            else
            {
                try
                {
                    return await File.ReadAllBytesAsync(filePath);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[HistoryTracker] LoadBytes failed: {e.Message}\n{filePath}");
                }
            }
            return null;
        }

        static async Task CopyAll(HistManager manager, HistRecords records)
        {
            var tasks = ListPool<Task>.Get();
            try
            {
                foreach (var record in records)
                {
                    var index = 0;
                    while (record.Paths.Count > index)
                    {
                        var path = record.Paths[index];
                        var fullPath = GetFullPath(path);
                        var pathBytes = await LoadBytes(fullPath);
                        if (pathBytes == null
                            || pathBytes.Length == 0)
                        {
                            record.Paths.RemoveAt(index);
                            continue;
                        }
                        var persistentDataPath = PersistentHistDataService.GetDataPath(Hist.DirectoryName);
                        var dstRootDir = Path.GetDirectoryName(persistentDataPath);
                        if (string.IsNullOrEmpty(dstRootDir))
                        {
                            continue;
                        }
                        var dstPath = Path.Combine(dstRootDir, path);
                        var dstDir = Path.GetDirectoryName(dstPath);
                        if (dstDir != null
                            && !Directory.Exists(dstDir))
                        {
                            Directory.CreateDirectory(dstDir);
                        }
                        var task = File.WriteAllBytesAsync(dstPath, pathBytes);
                        tasks.Add(task);
                        index++;
                    }
                    if (record.Paths.Count > 0)
                    {
                        manager.Records.AddOrSet(record);
                    }
                }
                await Task.WhenAll(tasks);
            }
            finally
            {
                manager.Save();
                ListPool<Task>.Release(tasks);
            }
        }
    }
}