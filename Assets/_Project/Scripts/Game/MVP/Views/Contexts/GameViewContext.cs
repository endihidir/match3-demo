using UnityEngine;

namespace Core.Views
{
    public interface IGameViewContext
    {
        public GridView GridView { get; }
    }
    
    public class GameViewContext : MonoBehaviour, IGameViewContext
    {
        [field: SerializeField] public GridView GridView { get; private set; }
    }
}