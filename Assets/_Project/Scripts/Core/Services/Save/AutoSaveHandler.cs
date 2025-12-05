using System.Collections.Generic;
using Core.MVPContext.Interfaces;

namespace Core.SaveSystem
{
    public static class AutoSaveHandler
    {
        private static readonly HashSet<IAutoSave> AutoSaveData = new();
        public static void Register(IAutoSave handler) => AutoSaveData.Add(handler);
        public static void Unregister(IAutoSave handler) => AutoSaveData.Remove(handler);
        
        [AutoSave]
        public static void SaveAll()
        {
            foreach (var data in AutoSaveData)
            {
                data?.Save();
            }
        }
    }
}