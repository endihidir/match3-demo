using Core.MVPContext;

namespace Core.Context
{
    public interface IContextOwner
    {
        public IMVPContext MVPContext { get; }
    }
}