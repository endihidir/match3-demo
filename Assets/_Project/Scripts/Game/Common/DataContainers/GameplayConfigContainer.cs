using NaughtyAttributes;
using UnityEngine;

namespace Core.Configs
{
    //[CreateAssetMenu(fileName = "GameplayConfigContainer", menuName = "Match3/Core/GameplayConfigContainer", order = 0)]
    public class GameplayConfigContainer : ScriptableObject
    { 
        [field: SerializeField, Required] public GridConfigContainerSO GridConfigContainer { get; private set; }
        public void Initialize()
        {
            
        }
    }
}