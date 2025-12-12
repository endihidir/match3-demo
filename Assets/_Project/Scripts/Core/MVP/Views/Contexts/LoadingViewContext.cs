using Core.Views;
using UnityEngine;

namespace Core.Context
{
    public interface ILoadingViewContext
    {
        SceneTransitionView SceneTransitionView { get; }
    }
    
    public class LoadingViewContext : MonoBehaviour, ILoadingViewContext
    {
        [field: SerializeField] public SceneTransitionView SceneTransitionView { get; private set; }
    }
}