using UnityEngine;

namespace Game.Configs
{
    public abstract class BaseGridObjectDataSO : ScriptableObject
    {
        public Sprite icon;
        public Vector2 spriteSizeMultiplier = Vector2.one;
        public bool isStationary;
    }
}