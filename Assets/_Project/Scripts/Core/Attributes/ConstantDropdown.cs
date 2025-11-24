using System;
using UnityEngine;

namespace Core.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ConstantDropdown : PropertyAttribute
    {
        public Type TargetType { get; }
        public ConstantDropdown(Type targetType) => TargetType = targetType;
    }
}
