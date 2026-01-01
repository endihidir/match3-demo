using Core.Item;
using UnityEngine;

namespace Core.Config
{
    //[CreateAssetMenu(fileName = "BoosterMergeConfig", menuName = "Match3/ItemConfigs/BoosterMergeConfig", order = 0)]
    public sealed class BoosterMergeConfigSO : ScriptableObject
    {
        [SerializeField] private MergeRule[] rules;

        public bool TryGetRule(BoosterType first, BoosterType second, out MergeRule rule)
        {
            var key = BoosterMergeKey.Create(first, second);

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