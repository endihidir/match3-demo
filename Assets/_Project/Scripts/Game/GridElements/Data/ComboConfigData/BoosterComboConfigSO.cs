using Core.Item;
using UnityEngine;

namespace Core.Configs
{
    //[CreateAssetMenu(fileName = "BoosterMergeConfig", menuName = "Match3/ItemConfigs/BoosterMergeConfig", order = 0)]
    public sealed class BoosterComboConfigSO : ScriptableObject
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

            rule = default;
            return false;
        }
    }
}