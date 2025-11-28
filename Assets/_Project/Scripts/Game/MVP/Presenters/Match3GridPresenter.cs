using Core.Item;
using Core.Models;

namespace Core.Presenters
{
    public interface IMatch3GridPresenter
    {
        void InitializeGrid();
        void Refresh();
    }
    
    public class Match3GridPresenter : GridPresenter<IMatch3GridModel, IGridItemBehaviour>, IMatch3GridPresenter
    {
        public Match3GridPresenter(IMatch3GridModel model) : base(model)
        {
            
        }

        public void InitializeGrid()
        {
           
        }

        public void Refresh()
        {
            
        }
    }
}