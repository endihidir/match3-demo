using Core.Models;
using UnityEngine;

namespace Core.Extensions
{
    public static class GridModelConfigExtensions
    {
        public static IBaseGridModel<T> SetScreenSidePaddingRatio<T>(this IBaseGridModel<T> model, float value) where T : class
        {
            model.ScreenSidePaddingRatio = value;
            return model;
        }
        
        public static IBaseGridModel<T> SetCellSpacingRatio<T>(this IBaseGridModel<T> model, float value) where T : class
        {
            model.CellSpacingRatio = value;
            return model;
        }

        public static IBaseGridModel<T> SetOriginOffset<T>(this IBaseGridModel<T> model, Vector3 value) where T : class
        {
            model.OriginOffset = value;
            return model;
        }

        public static IBaseGridModel<T> EnableDrawGizmos<T>(this IBaseGridModel<T> model, bool value) where T : class
        {
            model.DrawGizmos = value;
            return model;
        }

        public static IBaseGridModel<T> SetGizmosColor<T>(this IBaseGridModel<T> model, Color value) where T : class
        {
            model.GizmosColor = value;
            return model;
        }
    }
}