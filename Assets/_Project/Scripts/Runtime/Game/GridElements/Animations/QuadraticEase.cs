using DG.Tweening;
using UnityEngine;

namespace Game.Grid.Item
{
    public static class QuadraticEase
    {
        private const int Resolution = 20;

        private static readonly EaseFunction[] Functions = BuildFunctions();

        public static EaseFunction Get(float initialSlope)
        {
            var index = Mathf.Clamp(Mathf.RoundToInt(initialSlope * Resolution), 0, Resolution);
            return Functions[index];
        }

        private static EaseFunction[] BuildFunctions()
        {
            var functions = new EaseFunction[Resolution + 1];

            for (int i = 0; i <= Resolution; i++)
                functions[i] = new Curve(i / (float)Resolution).Evaluate;

            return functions;
        }

        private sealed class Curve
        {
            private readonly float _initialSlope;

            public Curve(float initialSlope) => _initialSlope = initialSlope;

            public float Evaluate(float time, float duration, float overshootOrAmplitude, float period)
            {
                var progress = duration > 0f ? Mathf.Clamp01(time / duration) : 1f;
                return _initialSlope * progress + (1f - _initialSlope) * progress * progress;
            }
        }
    }
}