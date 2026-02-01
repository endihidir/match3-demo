using Cysharp.Threading.Tasks;
using UnityEngine.UI;

namespace Core.UI
{
    public interface ILevelEndView
    {
        Button.ButtonClickedEvent OnClickNextButton { get; }
        Button.ButtonClickedEvent OnClickTryAgainButton { get; }
        void Initialize();
        UniTask OpenSuccessMenuViewAsync();
        UniTask OpenFailMenuViewAsync();
    }
}