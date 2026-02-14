using UnityEngine;

namespace Core.UI
{
    public class VerticalRocketFxView : RocketFxView
    {
        public override void UpdateRocketVisuals(float cellSize, float sizeMultiplier = .75f)
        {
            var verYSize = cellSize * sizeMultiplier;
            PositiveSideSpriteRenderer.size = new Vector2(cellSize, verYSize);
            NegativeSideSpriteRenderer.size = new Vector2(cellSize, verYSize);
            PositiveSideSpriteRenderer.transform.localPosition = new Vector3(0f, verYSize * 0.5f, 0f);
            NegativeSideSpriteRenderer.transform.localPosition = new Vector3(0f, verYSize * -0.5f, 0f);
        }
        public override void UpdateTargetPositions(Camera cam, Vector3 pos, float screenPadding = .2f)
        {
            var z = pos.z;
            var bottomEdge = cam.ViewportToWorldPoint(new Vector3(0.5f, -screenPadding, z));
            var topEdge = cam.ViewportToWorldPoint(new Vector3(0.5f, 1f + screenPadding, z));
            
            _positiveSideTargetPos = new Vector3(pos.x, topEdge.y, pos.z);
            _negativeSideTargetPos = new Vector3(pos.x, bottomEdge.y, pos.z);
        }
        protected override float GetDistance(Vector3 from, Vector3 to) => Mathf.Abs(to.y - from.y);
    }
}