
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace HistoryTracker
{
    public class PersistentHistoryDataService : IHistoryDataService
    {
        const string _recordsFileName = "Records.bytes";
        
        readonly string _rootDir;
        readonly string _dataPath;
        HistoryRecords _records;
        
        public PersistentHistoryDataService(string dirName)
        {
            _rootDir = Path.Combine(Application.persistentDataPath, dirName);
            _dataPath = Path.Combine(_rootDir, _recordsFileName);
            LoadRecords();
        }

        public HistoryRecords GetRecords() => _records;

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
                _records = JsonUtility.FromJson<HistoryRecords>(json);
            }
            finally
            {
                _records ??= new HistoryRecords();
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
        
        public HistoryRecord Add(IReadOnlyList<string> paths)
        {
            var record = _records.Create();
            Set(record, paths);
            return record;
        }

        public void Remove(HistoryRecord record)
        {
            if (record.Paths.Count > 0)
            {
                var path = record.Paths[0];
                var dir = Path.GetDirectoryName(path);
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
            _records.Remove(record);
        }
        
        public HistoryRecord Set(int recordId, IReadOnlyList<string> paths)
        {
            var record = _records.GetOrCreateById(recordId);
            Set(record, paths);
            return record;
        }
        
        public void Set(HistoryRecord record, IReadOnlyList<string> paths)
        {
            Assert.IsTrue(_records.HasRecord(record.Id));
            var dir = record.Id.ToString();
            
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
                var targetDir = Path.GetDirectoryName(target);
                if (!string.IsNullOrEmpty(targetDir)
                    && !Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }
                File.Copy(source, target, overwrite: true);
            }
        }
        
        public void Apply(HistoryRecord record, IReadOnlyList<string> paths)
        {
            if (record.Paths.Count != paths.Count)
            {
                throw new InvalidOperationException("The number of elements in record.Paths and paths do not match");
            }

            var baseDir = Path.Combine(_rootDir, record.Id.ToString());

            for (var i = 0; i < paths.Count; i++)
            {
                var recordPath = record.Paths[i];
                var userPath = paths[i];
                var expectedRelativePath = GetRelativePathFromRoot(recordPath, baseDir);
                var userRelativePath = GetRelativePathFromRoot(userPath, Application.persistentDataPath);

                if (!string.Equals(expectedRelativePath, userRelativePath, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"Path structures do not match. record: [{expectedRelativePath}] / paths: [{userRelativePath}]");
                }
            }
            
            for (var i = 0; i < paths.Count; i++)
            {
                var dest = record.Paths[i];
                var destDir = Path.GetDirectoryName(dest);
                if (!string.IsNullOrEmpty(destDir)
                    && !Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }
                File.Copy(paths[i], dest, overwrite: true);
            }
        }
        
        string GetRelativePathFromRoot(string fullPath, string root)
        {
            var rootPath = Path.GetFullPath(root);
            var targetPath = Path.GetFullPath(fullPath);
            if (!targetPath.StartsWith(rootPath))
            {
                return Path.GetFileName(targetPath);
            }
            return targetPath.Substring(rootPath.Length)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}