using Game.Grid.Item;
using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(fileName = "BoosterComboData", menuName = "Game/Gameplay/Grid/Data/BoosterComboData")]
    public sealed class BoosterComboDataSO : ScriptableObject
    {
        [SerializeField] private ComboRule[] rules;

        public bool TryGetRule(BoosterType first, BoosterType second, out ComboRule rule)
        {
            var key = BoosterComboKey.Create(first, second);

            foreach (var mergeRule in rules)
            {
                if (!mergeRule.Key.Equals(key)) continue;
                
                rule = mergeRule;
                
                return true;
            }

            rule = null;
            return false;
        }
    }
}