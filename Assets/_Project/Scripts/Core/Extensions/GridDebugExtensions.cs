using Core.Models;
using Core.Utils;
using UnityEngine;
using GridLayout = Core.Grid.GridLayout;

namespace Core.Extensions
{
    public static class GridDebugExtensions
    {
        public static void DrawGrid<T>(this IBaseGridModel<T> model, GridLayout layout, Camera camera = null) where T : class
        {
            if (!model.DrawGizmos) return;

            var cam = camera ? camera : Camera.main;
            if (!cam) return;

            Gizmos.color = model.GizmosColor;

            var gridSize = model.Size;

            for (int i = 0; i < model.Width * model.Height; i++)
            {
                var coordinate = CoordinateUtils.ToCoordinate(i, model.Width);

                var center = layout.GridToWorld(gridSize, cam, coordinate);

                var half = layout.CellSize * 0.5f;

                var topLeft     = new Vector3(center.x - half, center.y + half, center.z);
                var topRight    = new Vector3(center.x + half, center.y + half, center.z);
                var bottomRight = new Vector3(center.x + half, center.y - half, center.z);
                var bottomLeft  = new Vector3(center.x - half, center.y - half, center.z);

                Gizmos.DrawLine(topLeft, topRight);
                Gizmos.DrawLine(topRight, bottomRight);
                Gizmos.DrawLine(bottomRight, bottomLeft);
                Gizmos.DrawLine(bottomLeft, topLeft);
            }
        }
    }
}


/*public static bool TryGetGridObjectFromMousePosition<T>(this IBaseGridModel<T> model, out T obj, Camera camera = null) where T : class
{
    var cam = camera ? camera : Camera.main;
            
    if (!cam)
    {
        obj = null;
        return false;
    }

    var worldPosition = cam.ScreenToWorldPoint(Input.mousePosition).With(z: cam.nearClipPlane);
            
    var pos = model.WorldToGrid(worldPosition, false, cam);
            
    if (!model.IsInRange(pos))
    {
        obj = null;
        return false;
    }

    obj = model.GetGridObject(pos);
    return obj != null;
}*/