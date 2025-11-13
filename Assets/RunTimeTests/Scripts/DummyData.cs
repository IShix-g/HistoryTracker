#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using HistoryTracker;

namespace Tests
{
    public sealed class DummyData
    {
        public IHistoryDataService Service { get; private set; }
        
        public void Create()
        {
            Service = new PersistentHistoryDataService("Tests");
            
            var result = new List<Model>();
            for (var i = 1; i <= 3; i++)
            {
                var model = Model.Load(i);
                model.Number = i;
                model.Text = $"Model {i}";
                Model.Save(model);
                result.Add(model);
            }
            
            for (var i = 1; i <= 3; i++)
            {
                var paths = result.Select(x => x.GetFullPath()).ToArray();
                var record = Service.Add(paths);
                record.Title = $"Test {i}";
                record.Description = $"Description {i}";
            }

            Service.Save();
        }

        public void Release()
        {
            Service.DeleteAll();
            for (var i = 1; i <= 3; i++)
            {
                Model.Delete(i);
            }
        }
    }
}
#endif