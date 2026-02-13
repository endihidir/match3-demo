using UnityEngine;

namespace Core.UI
{
    public class HorizontalRocketFxView : RocketFxView
    {
        protected override float GetDistance(Vector3 from, Vector3 to) => Mathf.Abs(to.x - from.x);
        public override void CalculateTargetPositions()
        {
            _cam = Camera.main;
            
            if (!_cam) return;

            var pos = transform.position;
            var z = pos.z;
            var leftEdge = _cam.ViewportToWorldPoint(new Vector3(-ScreenPadding, 0.5f, z));
            var rightEdge = _cam.ViewportToWorldPoint(new Vector3(1f + ScreenPadding, 0.5f, z));
            
            _negativeSideTargetPos = new Vector3(leftEdge.x, pos.y, pos.z);
            _positiveSideTargetPos = new Vector3(rightEdge.x, pos.y, pos.z);
        }

        public override void CalculateRocketsSize(float cellSize)
        {
            var horXSize = cellSize * SizeMultiplier;
            NegativeSideSpriteRenderer.size = new Vector2(horXSize, cellSize);
            PositiveSideSpriteRenderer.size = new Vector2(horXSize, cellSize);
            NegativeSideSpriteRenderer.transform.localPosition = new Vector3(horXSize * -0.5f, 0f, 0f);
            PositiveSideSpriteRenderer.transform.localPosition = new Vector3(horXSize * 0.5f, 0f, 0f);
        }
    }
}