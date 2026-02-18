using Core.Modules;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Views
{
    public class AnimatedPanelModule : MonoBehaviour
    {
        [field: SerializeField] private FadeAnimationModule BgFadeAnimation { get; set; }
        [field: SerializeField] private BounceAnimationModule TitleBounceAnimation { get; set; }
        [field: SerializeField] private BounceAnimationModule ButtonBounceAnimation { get; set; }

        public async UniTask ShowAsync(float duration)
        {
            await UniTask.WaitForSeconds(0.5f);
            
            BgFadeAnimation.gameObject.SetActive(true);
            await BgFadeAnimation.FadeInAsync(.95f, duration);
            
            TitleBounceAnimation.gameObject.SetActive(true);
            ButtonBounceAnimation.gameObject.SetActive(true);
            
            TitleBounceAnimation.PlayBounce();
            ButtonBounceAnimation.PlayBounce();
        }

        public void Hide()
        {
            BgFadeAnimation.Graphic.color = BgFadeAnimation.Graphic.color.WithAlpha(0f);
            BgFadeAnimation.gameObject.SetActive(false);
            TitleBounceAnimation.gameObject.SetActive(false);
            ButtonBounceAnimation.gameObject.SetActive(false);
        }
    }
}