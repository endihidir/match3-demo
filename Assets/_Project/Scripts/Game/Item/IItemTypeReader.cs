namespace Core.Item
{
    public interface IItemTypeReader
    {
        GridItemKind ItemKind { get; }
        int TypeId { get; }
    }
}