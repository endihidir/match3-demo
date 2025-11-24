using Cysharp.Threading.Tasks;
using Core.Generated;
using Core.SceneService;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class PlayButton : MonoBehaviour
{
    [SerializeField] private Button _button;

    [Inject] private readonly ISceneLoadService _sceneLoadService;

    private void OnEnable()
    {
        _button.onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnClick);
    }

    private void OnClick() => InitGameScene().Forget();

    private async UniTask InitGameScene()
    {
        await _sceneLoadService.LoadSceneAsync(SceneType.GameScene, true);
    }
}
