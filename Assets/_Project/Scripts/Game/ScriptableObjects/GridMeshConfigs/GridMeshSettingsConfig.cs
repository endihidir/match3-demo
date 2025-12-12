using UnityEngine;

namespace Core.Config
{
    //[CreateAssetMenu(fileName = "GridMeshSettingsConfig", menuName = "Match3/GridConfigs/GridMeshSettings", order = 0)]
    public class GridMeshSettingsConfig : ScriptableObject
    {
        [field: SerializeField] public float FrameThickness { get; private set; } = 0.25f;
        [field: SerializeField] public float CornerSmoothness { get; private set; } = 1f;
        [field: SerializeField] public float MaxCellSize { get; private set; } = 3f;
        [field: SerializeField] public float ScreenSidePaddingRatio { get; private set; } = 5f;
    }
}