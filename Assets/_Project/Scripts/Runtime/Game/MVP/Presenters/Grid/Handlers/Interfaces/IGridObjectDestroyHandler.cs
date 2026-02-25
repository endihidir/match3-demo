using Game.Grid.Item;

namespace Game.Grid.Handlers
{
    public interface IGridObjectDestroyHandler
    {
        void DestroyGridObject(BaseGridObject obj);
        void ReleaseObject(BaseGridObject obj);  
    }
}

