
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace HistoryTracker
{
    internal abstract class FileBasedHistDataService : IHistDataService
    {
        public readonly string RootDir;
        public readonly string DataPath;
        HistRecords _records;

        protected FileBasedHistDataService(string dataPath)
        {
            DataPath = dataPath;
            RootDir = Path.GetDirectoryName(dataPath);
            LoadRecords();
        }

        public HistRecords GetRecords() => _records;

        void LoadRecords()
        {
            try
            {
                if (!File.Exists(DataPath))
                {
                    return;
                }
                var bytes = File.ReadAllBytes(DataPath);
                _records = Load(bytes);
            }
            finally
            {
                _records ??= new HistRecords();
                _records.Initialize();
            }
        }

        public static HistRecords Load(byte[] bytes)
        {
            var json = Encoding.UTF8.GetString(bytes);
            return JsonUtility.FromJson<HistRecords>(json);
        }

        public void Save()
        {
            var json = JsonUtility.ToJson(_records);
            if (string.IsNullOrEmpty(json)
                || json == "[]")
            {
                throw new InvalidOperationException("[HistoryTracker] Serialization result is invalid. Unable to save.");
            }
            Directory.CreateDirectory(RootDir);
            var bytes = Encoding.UTF8.GetBytes(json);
            File.WriteAllBytes(DataPath, bytes);
        }

        public void Delete() => File.Delete(DataPath);

        public void DeleteAll()
        {
            if (Directory.Exists(RootDir))
            {
                Directory.Delete(RootDir, true);
            }
        }

        public HistRecord Add(IReadOnlyList<string> paths)
        {
            var record = _records.Create();
            Set(record, paths);
            return record;
        }

        public HistRecord Set(string recordId, IReadOnlyList<string> paths)
        {
            var record = _records.GetOrCreateById(recordId);
            Set(record, paths);
            return record;
        }

        public void Set(HistRecord record, IReadOnlyList<string> paths)
        {
            Assert.IsTrue(_records.HasRecord(record.Id));
            var dir = record.Id;
            var rootPathLength = Application.persistentDataPath.Length;
            var newFilePaths = paths.Select(p =>
            {
                var relativePath = p.StartsWith(Application.persistentDataPath)
                    ? p.Substring(rootPathLength).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    : Path.GetFileName(p);
                return Path.Combine(dir, relativePath);
            }).ToList();

            record.Paths = newFilePaths;

            for (var i = 0; i < paths.Count; i++)
            {
                var source = paths[i];
                var target = Path.Combine(RootDir, newFilePaths[i]);
                EnsureCopy(source, target);
            }
        }

        public void Apply(HistRecord record, IReadOnlyList<string> paths, Action<bool> onFinished = null)
            => ApplyAsync(record, paths)
                .Handled(_ => onFinished?.Invoke(true), _ => onFinished?.Invoke(false));

        public async Task ApplyAsync(HistRecord record, IReadOnlyList<string> paths)
        {
            var idString = record.Id;
            var notAppliedRecordPaths = ListPool<string>.Get();
            var unusedPaths = ListPool<string>.Get();
            var tasks = ListPool<Task>.Get();

            try
            {
                var usedPathsFlags = new bool[paths.Count];

                foreach (var recordPath in record.Paths)
                {
                    if (!recordPath.StartsWith(idString, StringComparison.Ordinal))
                    {
                        notAppliedRecordPaths.Add(recordPath);
                        continue;
                    }

                    var relativePath = recordPath.Substring(idString.Length)
                        .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                    var copied = false;
                    for (var i = 0; i < paths.Count; i++)
                    {
                        var path = paths[i];
                        var normalizedPath = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                        if (!normalizedPath.EndsWith(relativePath, StringComparison.Ordinal))
                        {
                            continue;
                        }

                        var sourcePath = Path.Combine(RootDir, recordPath);
#if !UNITY_EDITOR
                        if (record.IsStreamingAssets
                            && !File.Exists(sourcePath))
                        {
                            sourcePath = StreamingAssetsRecordsInstaller.GetFullPath(recordPath);
                            var bytes = await StreamingAssetsRecordsInstaller.LoadBytes(sourcePath);
                            var task = File.WriteAllBytesAsync(path, bytes);
                            tasks.Add(task);
                        }
                        else
#endif
                        {
                            EnsureCopy(sourcePath, path);
                        }
                        copied = true;
                        usedPathsFlags[i] = true;
                        break;
                    }

                    if (!copied)
                    {
                        notAppliedRecordPaths.Add(recordPath);
                    }
                }

                for (var i = 0; i < paths.Count; i++)
                {
                    if (!usedPathsFlags[i])
                    {
                        unusedPaths.Add(paths[i]);
                    }
                }

                if (tasks.Count > 0)
                {
                    await Task.WhenAll(tasks);
                }
            }
            finally
            {
                if (notAppliedRecordPaths.Count > 0)
                {
                    Debug.LogWarning(
                        $"[HistoryTracker] Some elements in record.Paths were not copied during Apply. record.Id={record.Id}\n" +
                        string.Join("\n", notAppliedRecordPaths));
                }
                if (unusedPaths.Count > 0)
                {
                    Debug.LogWarning(
                        $"[HistoryTracker] Some elements in paths were not used during Apply. record.Id={record.Id}\n" +
                        string.Join("\n", unusedPaths));
                }
                ListPool<string>.Release(notAppliedRecordPaths);
                ListPool<string>.Release(unusedPaths);
                ListPool<Task>.Release(tasks);
            }
        }

        public IReadOnlyList<HistRecord> GetTrashRecords() => _records.GetTrashRecords();

        public void MoveToTrash(HistRecord record) => _records.MoveToTrash(record);

        public void RestoreFromTrash(HistRecord record) => _records.RestoreFromTrash(record);

        public void EmptyTrash()
        {
            foreach (var record in _records.GetTrashRecords())
            {
                var id = record.Id;
                var path = Path.Combine(RootDir, id);
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            _records.EmptyTrash();
        }

        void EnsureCopy(string source, string target)
        {
            var targetDir = Path.GetDirectoryName(target);
            if (!string.IsNullOrEmpty(targetDir)
                && !Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            File.Copy(source, target, overwrite: true);
        }
    }
}
