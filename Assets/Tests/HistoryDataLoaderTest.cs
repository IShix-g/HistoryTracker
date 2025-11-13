
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using HistoryTracker;

namespace Tests
{
    public class HistoryDataLoaderTest
    {
        const string _dirName = "Tests";
        const int _testModelLength = 3;

        [Test]
        public void SaveTest()
        {
            var models = PrepareModels();

            var testCount = 3;
            var loader = new PersistentHistoryDataService(_dirName);
            for (var i = 1; i <= testCount; i++)
            {
                var paths = models.Select(x => x.GetFullPath()).ToArray();
                var record = loader.Add(paths);
                record.Title = $"Test {i}";
                record.Description = $"Description {i}";
                Assert.That(record.Id, Is.EqualTo(i));
            }
            loader.Save();

            loader = new PersistentHistoryDataService(_dirName);
            Assert.That(loader.GetRecords().Length, Is.EqualTo(testCount));
            for (var i = 1; i <= testCount; i++)
            {
                var record = loader.GetRecords().GetById(i);
                Assert.That(record.Title, Is.EqualTo($"Test {i}"));
                Assert.That(record.Description, Is.EqualTo($"Description {i}"));
                Assert.That(record.Paths.Count, Is.EqualTo(_testModelLength));
            }

            loader.DeleteAll();
            DeleteModels();
        }

        List<Model> PrepareModels()
        {
            var result = new List<Model>();
            for (var i = 1; i <= _testModelLength; i++)
            {
                var model = Model.Load(i);
                model.Number = i;
                model.Text = $"Model {i}";
                Model.Save(model);
                result.Add(model);
            }
            return result;
        }

        void DeleteModels()
        {
            for (var i = 1; i <= _testModelLength; i++)
            {
                Model.Delete(i);
            }
        }
    }
}