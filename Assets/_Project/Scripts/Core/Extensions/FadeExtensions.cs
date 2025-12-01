using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Extensions
{
    public static class FadeExtensions
    {
        public static Tween DOFadeSafe(this Graphic graphic, float endValue, float duration)
        {
            return !graphic ? null : graphic.DOFade(endValue, duration);
        }
    
        public static Tween DOFadeSafe(this CanvasGroup canvasGroup, float endValue, float duration)
        {
            return !canvasGroup ? null : canvasGroup.DOFade(endValue, duration);
        }
    }
}