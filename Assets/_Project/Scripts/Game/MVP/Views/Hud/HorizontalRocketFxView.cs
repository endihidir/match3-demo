using UnityEngine;

namespace Core.UI
{
    public class HorizontalRocketFxView : RocketFxView
    {
        public override void UpdateRocketVisuals(float cellSize, float sizeMultiplier = .75f)
        {
            var horXSize = cellSize * sizeMultiplier;
            NegativeSideSpriteRenderer.size = new Vector2(horXSize, cellSize);
            PositiveSideSpriteRenderer.size = new Vector2(horXSize, cellSize);
            NegativeSideSpriteRenderer.transform.localPosition = new Vector3(horXSize * -0.5f, 0f, 0f);
            PositiveSideSpriteRenderer.transform.localPosition = new Vector3(horXSize * 0.5f, 0f, 0f);
        }
        public override void UpdateTargetPositions(Camera cam, Vector3 pos, float screenPadding = .2f)
        {
            var z = pos.z;
            var leftEdge = cam.ViewportToWorldPoint(new Vector3(-screenPadding, 0.5f, z));
            var rightEdge = cam.ViewportToWorldPoint(new Vector3(1f + screenPadding, 0.5f, z));
            _negativeSideTargetPos = new Vector3(leftEdge.x, pos.y, pos.z);
            _positiveSideTargetPos = new Vector3(rightEdge.x, pos.y, pos.z);
        }
        protected override float GetDistance(Vector3 from, Vector3 to) => Mathf.Abs(to.x - from.x);
    }
}