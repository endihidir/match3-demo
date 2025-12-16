using UnityEngine;

namespace Core.Item
{
    public interface IItemObject
    {
        SpriteRenderer SpriteRenderer { get; }
        ItemAnimation ItemAnimation { get; }
    }
}