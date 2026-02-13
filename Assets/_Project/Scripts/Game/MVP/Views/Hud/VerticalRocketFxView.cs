using UnityEngine;

namespace Core.UI
{
    public class VerticalRocketFxView : RocketFxView
    {
        public override void CalculateTargetPositions()
        {
            _cam = Camera.main;
            if (!_cam) return;
            
            var pos = transform.position;
            var z = pos.z;
            var bottomEdge = _cam.ViewportToWorldPoint(new Vector3(0.5f, -ScreenPadding, z));
            var topEdge = _cam.ViewportToWorldPoint(new Vector3(0.5f, 1f + ScreenPadding, z));
            
            _positiveSideTargetPos = new Vector3(pos.x, topEdge.y, pos.z);
            _negativeSideTargetPos = new Vector3(pos.x, bottomEdge.y, pos.z);
        }
        
        protected override float GetDistance(Vector3 from, Vector3 to) => Mathf.Abs(to.y - from.y);

        public override void CalculateRocketsSize(float cellSize)
        {
            var verYSize = cellSize * SizeMultiplier;
            PositiveSideSpriteRenderer.size = new Vector2(cellSize, verYSize);
            NegativeSideSpriteRenderer.size = new Vector2(cellSize, verYSize);
            PositiveSideSpriteRenderer.transform.localPosition = new Vector3(0f, verYSize * 0.5f, 0f);
            NegativeSideSpriteRenderer.transform.localPosition = new Vector3(0f, verYSize * -0.5f, 0f);
        }
    }
}