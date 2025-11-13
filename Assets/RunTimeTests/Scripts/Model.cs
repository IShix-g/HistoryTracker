
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Tests
{
    [Serializable]
    public sealed class Model
    {
        public static readonly string DataPath = Path.Combine(Application.persistentDataPath, "TestModel_{0}.bytes");

        public int Id;
        public int Number;
        public string Text;
        
        public string GetFullPath() => GetFullPath(Id);
        
        public static string GetFullPath(int id) => string.Format(DataPath, id);
        
        public static Model Load(int id)
        {
            var path = GetFullPath(id);
            var records = default(Model);
            try
            {
                if (File.Exists(path))
                {
                    var bytes = File.ReadAllBytes(path);
                    var json = Encoding.UTF8.GetString(bytes);
                    records = JsonUtility.FromJson<Model>(json);
                }
            }
            finally
            {
                records ??= new Model();
                records.Id = id;
            }
            return records;
        }

        public static void Save(Model model)
        {
            var path = GetFullPath(model.Id);
            var json = JsonUtility.ToJson(model);
            if (string.IsNullOrEmpty(json)
                || json == "[]")
            {
                throw new InvalidOperationException("Serialization result is invalid. Unable to save.");
            }
            var bytes = Encoding.UTF8.GetBytes(json);
            File.WriteAllBytes(path, bytes);
        }

        public static void Delete(int id)
        {
            var path = string.Format(DataPath, id);
            File.Delete(path);
        }
    }
}