
using System.IO;
using System.Linq;
using NUnit.Framework;
using HistoryTracker;
using UnityEngine;

namespace Tests
{
    public class HistoryDataLoaderTest
    {
        static readonly string[] s_paths =
        {
            Path.Combine(Application.persistentDataPath, "EditorTest", "test1.bytes"),
            Path.Combine(Application.persistentDataPath, "EditorTest", "test2.bytes"),
            Path.Combine(Application.persistentDataPath, "EditorTest", "test3.bytes")
        };
        const string _dirName = "EditorTest";

        [Test]
        public void SaveTest()
        {
            var repository = new ModelRepository();
            repository.Load(s_paths);

            var testCount = 3;
            var service = new PersistentHistDataService(_dirName);
            var paths = repository.Paths.Values.ToArray();
            
            for (var i = 1; i <= testCount; i++)
            {
                var record = service.Add(paths);
                record.Title = $"Test {i}";
                record.Description = $"Description {i}";
            }
            service.Save();

            service = new PersistentHistDataService(_dirName);
            Assert.That(service.GetRecords().Length, Is.EqualTo(testCount));
            for (var i = 1; i <= testCount; i++)
            {
                var record = service.GetRecords().GetAt(i - 1);
                Assert.That(record.Title, Is.EqualTo($"Test {i}"));
                Assert.That(record.Description, Is.EqualTo($"Description {i}"));
                Assert.That(record.Paths.Count, Is.EqualTo(s_paths.Length));
            }

            service.DeleteAll();
            repository.DeleteAll();
        }
    }
}