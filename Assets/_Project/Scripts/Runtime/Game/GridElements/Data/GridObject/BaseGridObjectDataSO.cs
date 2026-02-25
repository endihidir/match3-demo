using UnityEngine;

namespace Game.Configs
{
    public abstract class BaseGridObjectDataSO : ScriptableObject
    {
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public Vector2 SpriteSizeMultiplier  { get; private set; } = Vector2.one;
        [field: SerializeField]  public bool IsStationary { get; private set; }
        [field: SerializeField] public bool IsCollectible { get; private set; }

        public virtual Sprite GetCollectibleSprite() => Icon;
    }
}