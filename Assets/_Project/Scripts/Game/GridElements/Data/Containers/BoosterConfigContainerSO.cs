using Core.Handlers;
using Core.Item;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(fileName = "BoosterConfigContainer", menuName = "Match3/ItemConfigs/BoosterConfigContainer", order = -1)]
    public class BoosterConfigContainerSO : BaseItemConfigContainerSO
    {
        [field: SerializeField] public BoosterComboConfigSO BoosterComboConfigSo { get; private set; }
        [field: SerializeField] public EnumConfigMap<BoosterType, BoosterDataSO> Configs { get; private set; }
        
        public float GetAnimationSpeed(BoosterActionContext action) => action.BoosterAction switch
        {
            RocketHorizontalAction => Configs.Get(BoosterType.RocketHorizontal).AnimationSpeed,
            RocketVerticalAction => Configs.Get(BoosterType.RocketVertical).AnimationSpeed,
            BombAction => Configs.Get(BoosterType.Bomb).AnimationSpeed,
            _ => 0
        };
        
        [Button]
        protected void EnsureAllKeysExist() => Configs.EnsureAllKeysExist();
    }
}