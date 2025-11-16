
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace HistoryTracker
{
    internal abstract class FileBasedHistDataService : IHistDataService
    {
        readonly string _rootDir;
        readonly string _dataPath;
        HistRecords _records;

        protected FileBasedHistDataService(string rootDir, string dataPath)
        {
            _rootDir = rootDir;
            _dataPath = dataPath;
            LoadRecords();
        }

        public HistRecords GetRecords() => _records;

        void LoadRecords()
        {
            try
            {
                if (!File.Exists(_dataPath))
                {
                    return;
                }
                var bytes = File.ReadAllBytes(_dataPath);
                var json = Encoding.UTF8.GetString(bytes);
                _records = JsonUtility.FromJson<HistRecords>(json);
            }
            finally
            {
                _records ??= new HistRecords();
            }
        }

        public void Save()
        {
            var json = JsonUtility.ToJson(_records);
            if (string.IsNullOrEmpty(json)
                || json == "[]")
            {
                throw new InvalidOperationException("Serialization result is invalid. Unable to save.");
            }
            Directory.CreateDirectory(_rootDir);
            var bytes = Encoding.UTF8.GetBytes(json);
            File.WriteAllBytes(_dataPath, bytes);
        }

        public void Delete() => File.Delete(_dataPath);

        public void DeleteAll()
        {
            if (Directory.Exists(_rootDir))
            {
                Directory.Delete(_rootDir, true);
            }
        }
        
        public HistRecord Add(IReadOnlyList<string> paths)
        {
            var record = _records.Create();
            Set(record, paths);
            return record;
        }

        public void Remove(HistRecord record)
        {
            var id = record.Id;
            var path = Path.Combine(_rootDir, id);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            _records.Remove(record);
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
                var relPath = p.StartsWith(Application.persistentDataPath)
                    ? p.Substring(rootPathLength).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    : Path.GetFileName(p);
                return Path.Combine(dir, relPath);
            }).ToList();

            record.Paths = newFilePaths;

            for (var i = 0; i < paths.Count; i++)
            {
                var source = paths[i];
                var target = Path.Combine(_rootDir, newFilePaths[i]);
                EnsureCopy(source, target);
            }
        }
        
        public void Apply(HistRecord record, IReadOnlyList<string> paths)
        {
            var idString = record.Id;
            var notAppliedRecordPaths = ListPool<string>.Get();
            var unusedPaths = ListPool<string>.Get();
            
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
                        var normalizedPath = path
                            .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                        
                        if (!normalizedPath.EndsWith(relativePath, StringComparison.Ordinal))
                        {
                            continue;
                        }

                        var source = Path.Combine(_rootDir, recordPath);
                        EnsureCopy(source, path);
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
            }
            finally
            {
                if (notAppliedRecordPaths.Count > 0)
                {
                    Debug.LogWarning(
                        $"Some elements in record.Paths were not copied during Apply. record.Id={record.Id}\n" +
                        string.Join("\n", notAppliedRecordPaths));
                }
                if (unusedPaths.Count > 0)
                {
                    Debug.LogWarning(
                        $"Some elements in paths were not used during Apply. record.Id={record.Id}\n" +
                        string.Join("\n", unusedPaths));
                }
                ListPool<string>.Release(notAppliedRecordPaths);
                ListPool<string>.Release(unusedPaths);
            }
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