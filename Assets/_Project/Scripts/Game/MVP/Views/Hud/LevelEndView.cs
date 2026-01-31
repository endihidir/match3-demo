using Core.Utils;
using UnityEngine;

namespace Core.UI
{
    public class LevelEndView : MonoBehaviour, ILevelEndView
    {
        
        public void OpenSuccessMenuView()
        {
            EditorLogger.LogError("AAAAAAAAAAAAAAA");
        }

        public void OpenFailMenuView()
        {
            EditorLogger.LogError("BBBBBBBBBBBBBB");
        }
    }
}