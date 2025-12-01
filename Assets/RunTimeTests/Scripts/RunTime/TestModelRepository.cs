#if DEBUG
using System.Collections.Generic;
using System.Linq;
using HistoryTracker;

namespace Tests
{
    public sealed class TestModelRepository : ModelRepository, IHistSaveDataHandler
    {
        HistRecordInfo IHistSaveDataHandler.OnBeforeSave()
        {
            for (var i = 0; i < Models.Count; i++)
            {
                var model = Models[i];
                var path = GetFullPath(model.Id);
                Save(model, path);
            }
            var title = "Saved Count: " + Models[0].SaveCount;
            var description = "[Test]";
            return new HistRecordInfo(title, description);
        }

        IReadOnlyList<string> IHistSaveDataHandler.GetSaveFilePaths() => Paths.Values.ToList();

        void IHistSaveDataHandler.ApplyData(HistAppliedInfo info) => Restored();
    }
}
#endif
