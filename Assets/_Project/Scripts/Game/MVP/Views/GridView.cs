using Core.Config;
using UnityEngine;

namespace Core.Views
{
    public interface IGridView
    {
        Camera Cam { get; }
        Transform GridRoot { get; }
        MeshFilter GridMeshFilter { get; }
        GridMeshSettingsConfig MeshSettings { get; }
    }
    
    public class GridView : MonoBehaviour, IGridView
    {
        [field: SerializeField] public Camera Cam { get; private set; }
        [field: SerializeField] public Transform GridRoot { get; private set; }
        [field: SerializeField] public MeshFilter GridMeshFilter { get; private set; }
        
        [field: SerializeField] public GridMeshSettingsConfig MeshSettings { get; private set; }
    }
}