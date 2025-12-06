using Core.Models;
using Core.MVPContext.Interfaces;

namespace Core.Presenters
{
    public class GridPresenter<TModel, TItem> : IPresenter where TModel : IGridModel<TItem> where TItem : class
    {
        
    }
}
