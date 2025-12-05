using System;

namespace Core.SaveSystem
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class AutoSaveAttribute : Attribute { }
}