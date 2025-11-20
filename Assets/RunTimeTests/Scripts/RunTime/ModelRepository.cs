#if DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Tests
{
    public class ModelRepository
    {
        public event Action OnRestored = delegate { };
        
        public List<Model> Models { get; } = new ();
        public Dictionary<int, string> Paths { get; } = new ();
        
        public void Load(IReadOnlyList<string> modelPaths)
        {
            for (var i = 0; i < modelPaths.Count; i++)
            {
                var path = modelPaths[i];
                var id = i + 1;
                var model = Load(id, path);
                if (!File.Exists(path))
                {
                    Save(model, path);
                }
            }
        }
        
        public string GetFullPath(int id) => Paths[id];
        
        public Model Load(int id, string path)
        {
            var records = default(Model);
            try
            {
                if (File.Exists(path))
                {
                    var bytes = File.ReadAllBytes(path);
                    var json = Encoding.UTF8.GetString(bytes);
                    records = JsonUtility.FromJson<Model>(json);
                    SetOrAdd(records);
                    Paths[records.Id] = path;
                }
            }
            finally
            {
                records ??= new Model();
                records.Id = id;
            }
            return records;
        }

        public void Save(Model model, string path)
        {
            model.SaveCount++;
            var json = JsonUtility.ToJson(model);
            if (string.IsNullOrEmpty(json)
                || json == "[]")
            {
                throw new InvalidOperationException("Serialization result is invalid. Unable to save.");
            }
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var bytes = Encoding.UTF8.GetBytes(json);
            File.WriteAllBytes(path, bytes);
            SetOrAdd(model);
            Paths[model.Id] = path;
        }

        void SetOrAdd(Model model)
        {
            var index = Models.FindIndex(x => x.Id == model.Id);
            if (index >= 0)
            {
                Models[index] = model;
            }
            else
            {
                Models.Add(model);
            }
        }
        
        public void Delete(int id)
        {
            var index = Models.FindIndex(x => x.Id == id);
            if (index < 0)
            {
                return;
            }
            var path = GetFullPath(id);
            File.Delete(path);
            Models.RemoveAt(index);
            Paths.Remove(id);
        }
        
        public void DeleteAll()
        {
            while (Models.Count > 0)
            {
                var model = Models[^1];
                Delete(model.Id);
            }
        }

        protected void Restored()
        {
            for (var i = 0; i < Paths.Count; i++)
            {
                var id = i + 1;
                var path = Paths[id];
                Models[i] = Load(id, path);
            }
            OnRestored();
        }
    }
}
#endif