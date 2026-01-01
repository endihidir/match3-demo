using UnityEngine;

namespace Core.Config
{
    public abstract class BaseItemDataSO : ScriptableObject
    {
        public Sprite icon;
        public Vector2 spriteSizeMultiplier = Vector2.one;
        public bool isStationary;
    }
}