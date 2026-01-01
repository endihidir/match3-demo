using Core.Item;
using UnityEngine;

namespace Core.Handlers
{
    public struct MovePlan
    {
        public readonly Vector2Int From;
        public readonly Vector2Int To;
        public readonly BaseGridObject Item;
        public readonly MoveKind Kind;

        public MovePlan(Vector2Int from, Vector2Int to, BaseGridObject item, MoveKind kind)
        {
            From = from;
            To = to;
            Item = item;
            Kind = kind;
        }
    }
}