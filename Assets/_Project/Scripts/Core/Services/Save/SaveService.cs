using System.IO;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;


namespace Core.SaveSystem
{
    public interface ISaveService
    {
        public void SaveToJson<T>(string key, T data);
        public T LoadFromJson<T>(string key, T defaultData = default, bool autoSaveDefaultData = true);
        public void SaveToPrefs<T>(string key, T data);
        public T LoadFromPrefs<T>(string key, T defaultData = default);
    }
    public class SaveService : ISaveService
    {
        private const string DIRECTORY_NAME = "JsonData";

#if UNITY_EDITOR
        private static string DirectoryPath => $"{Application.dataPath}/{DIRECTORY_NAME}";
#else
        private static string DirectoryPath => $"{Application.persistentDataPath}/{DirectoryName}";
#endif
        
        public void Initialize() { }

        public void SaveToJson<T>(string key, T data)
        {
            EnsureDirectoryExists();

            var filePath = GetFilePath(key);

            var jsonData = JsonUtility.ToJson(data);
            
            File.WriteAllText(filePath, jsonData);

#if UNITY_EDITOR
            if(!Application.isPlaying)
                AssetDatabase.Refresh();
#endif
        }

        public T LoadFromJson<T>(string key, T defaultData = default, bool autoSaveDefaultData = true)
        {
            EnsureDirectoryExists();

            var filePath = GetFilePath(key);

            if (!File.Exists(filePath))
            {
                if (defaultData is not null && autoSaveDefaultData)
                {
                    SaveToJson(key, defaultData);
                }
                
                return defaultData;
            }

            var jsonData = File.ReadAllText(filePath);
                
            var data = JsonUtility.FromJson<T>(jsonData);
            
            return data;
        }

        public void SaveToPrefs<T>(string key, T data)
        {
            var jsonData = JsonUtility.ToJson(data);
            
            PlayerPrefs.SetString(key, jsonData);
        }

        public T LoadFromPrefs<T>(string key, T defaultData = default)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                SaveToPrefs(key, defaultData);
            }
            
            var jsonData = PlayerPrefs.GetString(key, string.Empty);
                
            var data = JsonUtility.FromJson<T>(jsonData);
            
            return data;
        }
        
        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(DirectoryPath)) 
                Directory.CreateDirectory(DirectoryPath);
        }

        public static void ClearJsonData()
        {
            var files = Directory.GetFiles(DirectoryPath).Select(Path.GetFileName).ToArray();

            foreach (string key in files)
            {
                var filePath = $"{DirectoryPath}/{key}";
                
                File.Delete(filePath);
            }
            
#if UNITY_EDITOR
            if(!Application.isPlaying)
                AssetDatabase.Refresh();
#endif
        }
        
        private string GetFilePath(string key) => Path.Combine(DirectoryPath, $"{key}.json");
        public void Dispose() { }
    }
}