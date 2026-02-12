using Core.Extensions;
using UnityEngine;

namespace Core.UI
{
    public class HorizontalRocketFxView : RocketFxView
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
            var leftEdge = _cam.ViewportToWorldPoint(new Vector3(-ScreenPadding, 0.5f, z));
            var rightEdge = _cam.ViewportToWorldPoint(new Vector3(1f + ScreenPadding, 0.5f, z));
            
            _negativeSideTargetPos = new Vector3(leftEdge.x, startPos.y, startPos.z);
            _positiveSideTargetPos = new Vector3(rightEdge.x, startPos.y, startPos.z);
        }
        
        protected override float GetDistance(Vector3 from, Vector3 to) => Mathf.Abs(to.x - from.x);

        private void CalculateRocketsSizes(float cellSize)
        {
            var horXSize = cellSize * SizeMultiplier;
            NegativeSideSpriteRenderer.size = new Vector2(horXSize, cellSize);
            PositiveSideSpriteRenderer.size = new Vector2(horXSize, cellSize);
            NegativeSideSpriteRenderer.transform.localPosition = NegativeSideSpriteRenderer.transform.localPosition.With(x: horXSize * -0.5f);
            PositiveSideSpriteRenderer.transform.localPosition = PositiveSideSpriteRenderer.transform.localPosition.With(x: horXSize * 0.5f);
        }
    }
}