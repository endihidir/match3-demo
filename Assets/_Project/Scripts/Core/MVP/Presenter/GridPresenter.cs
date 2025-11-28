using Core.Models;

namespace Core.Presenters
{
    public class GridPresenter<TModel, TItem> where TModel : IGridModel<TItem> where TItem : class
    {
        protected readonly TModel Model;
        protected GridPresenter(TModel model)
        {
            Model = model;
        }
    }
}
