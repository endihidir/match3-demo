using UnityEngine;

namespace Core.Configs
{
    //[CreateAssetMenu(fileName = "GridLayoutSettingsConfig", menuName = "Match3/GridConfigs/GridLayoutSettings", order = 0)]
    public class GridLayoutSettingsSO : ScriptableObject
    {
        [field: SerializeField] public float MaxCellSize { get; private set; } = 3f;
        [field: SerializeField] public float ScreenSidePaddingRatio { get; private set; } = 5f;
        [field: SerializeField] public float CellSpacingRatio { get; private set; } = 0f;
    }
}