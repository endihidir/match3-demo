using Core.Models;
using UnityEngine;

namespace Core.Extensions
{
    public static class GridModelConfigExtensions
    {
        public static IGridModel<T> SetScreenSidePaddingRatio<T>(this IGridModel<T> model, float value) where T : class
        {
            model.ScreenSidePaddingRatio = value;
            return model;
        }
        
        public static IGridModel<T> SetCellSpacingRatio<T>(this IGridModel<T> model, float value) where T : class
        {
            model.CellSpacingRatio = value;
            return model;
        }

        public static IGridModel<T> SetOriginOffset<T>(this IGridModel<T> model, Vector3 value) where T : class
        {
            model.OriginOffset = value;
            return model;
        }

        public static IGridModel<T> EnableDrawGizmos<T>(this IGridModel<T> model, bool value) where T : class
        {
            model.DrawGizmos = value;
            return model;
        }

        public static IGridModel<T> SetGizmosColor<T>(this IGridModel<T> model, Color value) where T : class
        {
            model.GizmosColor = value;
            return model;
        }
    }
}