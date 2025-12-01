using Core.SceneService;

namespace Core.Utils
{
    public static class AsyncOperationUtils
    {
        public static float CombinedProgress(AsyncOperationGroup op, AsyncOperationHandleGroup handle)
        {
            var nOp = op.Operations.Count;
            var nHd = handle.Handles.Count;

            if (nOp == 0 && nHd == 0) return 0f;
            if (nOp == 0) return handle.Progress;
            if (nHd == 0) return op.Progress;

            return (op.Progress * nOp + handle.Progress * nHd) / (nOp + nHd);
        }
    }
}