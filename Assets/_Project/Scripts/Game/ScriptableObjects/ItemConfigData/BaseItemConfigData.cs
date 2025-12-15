using UnityEngine;

namespace Core.Config
{
    public abstract class BaseItemConfigData : ScriptableObject
    {
        public Sprite icon;
        public Vector2 spriteSizeMultiplier = Vector2.one;
    }
}