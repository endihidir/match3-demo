using Core.Config;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Configs
{
    //[CreateAssetMenu(fileName = "GameplayConfigContainer", menuName = "Match3/Core/GameplayConfigContainer", order = 0)]
    public class GameplayConfigContainer : ScriptableObject
    {
        [field: SerializeField, Required] public GameplayBootstrapperConfig GameplayBootstrapperConfig { get; private set; }
        [field: SerializeField, Required] public ItemConfigContainerSO ItemConfigContainer { get; private set; }
        [field: SerializeField] public FillStrategySettingsSO FillStrategySettings { get; private set; }
        [field: SerializeField] public FillItemDecisionSettingsSO FillItemDecisionSettings { get; private set; }
        public void Initialize()
        {
            
        }
    }
}