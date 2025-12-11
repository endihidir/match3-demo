using Core.Level;

namespace Core.Item
{
    public interface IItemTypeReader
    {
        GridItemKind ItemKind { get; }
        int TypeId { get; }
        GridObjectTypeData TypeData => new(ItemKind, TypeId);
    }
}