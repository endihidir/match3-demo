using UnityEngine;

namespace Core.Config
{
    //[CreateAssetMenu(fileName = "GridMeshSettingsConfig", menuName = "Match3/GridConfigs/GridMeshSettings", order = 0)]
    public class GridMeshSettingsSO : ScriptableObject
    {
        [field: SerializeField] public float FrameThickness { get; private set; } = 0.25f;
        [field: SerializeField] public float CornerSmoothness { get; private set; } = 1f;
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