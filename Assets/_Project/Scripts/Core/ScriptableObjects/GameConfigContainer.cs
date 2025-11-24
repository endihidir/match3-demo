using Core.Config;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Configs
{
    //[CreateAssetMenu(fileName = "GameConfigContainer", menuName = "Match3/Core/GameConfigContainer", order = 0)]
    public class GameConfigContainer : ScriptableObject
    {
        [field: SerializeField, Required] public GameBootstrapperConfig GameBootstrapperConfig { get; private set; }
        [field: SerializeField, Required] public ItemConfigContainer ItemConfigContainer { get; private set; }
        
        public void Initialize()
        {
            
        }
    }
}