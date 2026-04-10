using Game.Grid.Item;

namespace Game.Grid.Handlers
{
    public interface IGridObjectDestroyHandler
    {
        void DestroyGridObject(BaseGridObject obj);
        void PlayBlastFx(BaseGridObject obj);
        void RemoveObject(BaseGridObject obj);
        void ReleaseObject(BaseGridObject obj);
    }
}

