using Game.Grid.Item;
using Game.Grid.Contexts;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(fileName = "BoosterConfigContainer", menuName = "Game/Gameplay/Grid/Containers/BoosterConfigContainer")]
    public class BoosterConfigContainerSO : BaseGridObjectConfigContinerSO
    {
        [field: SerializeField] public BoosterComboDataSO BoosterComboDataSo { get; private set; }
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