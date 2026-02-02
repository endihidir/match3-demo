using Core.Views;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Core.UI
{
    public class LevelEndMenuAnimationView : MonoBehaviour
    {
        [field: SerializeField] private FadeAnimationView BgFadeAnimation { get; set; }
        [field: SerializeField] private BounceAnimationView TitleBounceAnimation { get; set; }
        [field: SerializeField] private BounceAnimationView ButtonBounceAnimation { get; set; }

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