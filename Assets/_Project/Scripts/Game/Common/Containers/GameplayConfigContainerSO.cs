using NaughtyAttributes;
using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(fileName = "GameplayConfigContainer", menuName = "Game/Containers/GameplayConfigContainer")]
    public class GameplayConfigContainerSO : ScriptableObject
    { 
        [field: SerializeField, Required] public GridConfigContainerSO GridConfigContainer { get; private set; }
    }
}