using UnityEngine;

namespace Core.Utils
{
    public static class UIWorldSpaceUtils
    {
        public static Vector2 WorldSizeToUISize(Vector2 spriteSize, Camera cam, Canvas canvas)
        {
            var p0 = cam.WorldToScreenPoint(Vector3.zero);
            var px = cam.WorldToScreenPoint(new Vector3(spriteSize.x, 0f, 0f));
            var py = cam.WorldToScreenPoint(new Vector3(0f, spriteSize.y, 0f));

            var pixelW = Mathf.Abs(px.x - p0.x);
            var pixelH = Mathf.Abs(py.y - p0.y);

           
            var sf = canvas != null ? canvas.scaleFactor : 1f;
            return new Vector2(pixelW / sf, pixelH / sf);
        }
        
        public static Vector2 UISizeToWorldSize(Vector2 uiSizeDelta, Camera cam, Canvas canvas)
        {
            var sf = canvas ? canvas.scaleFactor : 1f;
            
            var pixelSize = uiSizeDelta * sf;
            
            var p0 = cam.ScreenToWorldPoint(Vector3.zero);
            var px = cam.ScreenToWorldPoint(new Vector3(pixelSize.x, 0f, 0f));
            var py = cam.ScreenToWorldPoint(new Vector3(0f, pixelSize.y, 0f));

            var worldW = Mathf.Abs(px.x - p0.x);
            var worldH = Mathf.Abs(py.y - p0.y);

            return new Vector2(worldW, worldH);
        }
    }
}