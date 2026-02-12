using Core.Extensions;
using UnityEngine;

namespace Core.UI
{
    public class VerticalRocketFxView : RocketFxView
    {
        public override void Initialize(Vector3 startPos, float cellSize)
        {
            CalculateRocketsSizes(cellSize);
            CalculateTargetPositions(startPos);
        }
        
        private void CalculateTargetPositions(Vector3 startPos)
        {
            _cam = Camera.main;
            if (!_cam) return;
            
            var z = startPos.z;
            var bottomEdge = _cam.ViewportToWorldPoint(new Vector3(0.5f, -ScreenPadding, z));
            var topEdge = _cam.ViewportToWorldPoint(new Vector3(0.5f, 1f + ScreenPadding, z));
            
            _positiveSideTargetPos = new Vector3(startPos.x, topEdge.y, startPos.z);
            _negativeSideTargetPos = new Vector3(startPos.x, bottomEdge.y, startPos.z);
        }
        
        protected override float GetDistance(Vector3 from, Vector3 to) => Mathf.Abs(to.y - from.y);

        private void CalculateRocketsSizes(float cellSize)
        {
            var verYSize = cellSize * SizeMultiplier;
            PositiveSideSpriteRenderer.size = new Vector2(cellSize, verYSize);
            NegativeSideSpriteRenderer.size = new Vector2(cellSize, verYSize);
            PositiveSideSpriteRenderer.transform.localPosition = PositiveSideSpriteRenderer.transform.localPosition.With(y: verYSize * 0.5f);
            NegativeSideSpriteRenderer.transform.localPosition = NegativeSideSpriteRenderer.transform.localPosition.With(y: verYSize * -0.5f);
        }
    }
}