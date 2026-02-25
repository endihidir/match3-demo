using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(fileName = "GridMeshConfig", menuName = "Game/Gameplay/Grid/View/GridMeshConfig")]
    public sealed class GridMeshConfigSO : ScriptableObject
    {
        [field: SerializeField] public float FrameThickness { get; private set; } = 0.32f;
        [field: SerializeField] public float CornerSmoothness { get; private set; } = 1.2f;
        [field: SerializeField] public float FrameOffset { get; private set; } = -0.02f;
        [field: SerializeField] public MeshQualityPreset MeshQuality { get; private set; } = MeshQualityPreset.High;
        public int GetCornerSegments()
        {
            return MeshQuality switch
            {
                MeshQualityPreset.Low => 4,
                MeshQualityPreset.Medium => 8,
                _ => 16
            };
        }
        public enum MeshQualityPreset { Low, Medium, High }
    }
}