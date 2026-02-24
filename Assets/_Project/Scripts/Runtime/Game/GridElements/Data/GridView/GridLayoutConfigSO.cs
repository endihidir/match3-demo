using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(fileName = "GridLayoutConfig", menuName = "Game/Gameplay/Grid/View/GridLayoutConfig")]
    public sealed class GridLayoutConfigSO : ScriptableObject
    {
        [field: SerializeField] public float MaxCellSize { get; private set; } = 3f;
        [field: SerializeField] public float ScreenSidePaddingRatio { get; private set; } = 5f;
        [field: SerializeField] public float CellSpacingRatio { get; private set; } = 0f;
    }
}