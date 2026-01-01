using Core.Config;
using Core.Models;

namespace Core.Handlers
{
    public interface IRefillStrategyHandler
    {
        void Initialize(RefillSettingsSO settingsSo);
        IRefillStrategy SelectStrategy(IGridModel model);
    }
}