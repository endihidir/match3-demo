using Core.Utils;
using UnityEngine;

namespace Core.UI
{
    public class LevelEndView : MonoBehaviour, ILevelEndView
    {
        
        public void OpenLevelSuccessPanel()
        {
            EditorLogger.LogError("AAAAAAAAAAAAAAA");
        }

        public void OpenLevelFailPanel()
        {
            EditorLogger.LogError("BBBBBBBBBBBBBB");
        }
    }
}