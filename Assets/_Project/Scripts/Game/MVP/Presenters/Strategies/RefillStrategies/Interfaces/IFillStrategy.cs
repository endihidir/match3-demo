using System.Threading.Tasks;
using Core.Models;

namespace Core.Handlers
{
    public interface IFillStrategy
    {
        bool CanRefill(IGridModel model);
        IFillStrategy Execute(GridStateContext context);
        Task WaitAnimationsAsync();
    }
}