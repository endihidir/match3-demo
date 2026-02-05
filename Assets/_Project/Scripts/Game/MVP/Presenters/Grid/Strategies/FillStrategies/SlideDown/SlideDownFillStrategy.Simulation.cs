using Core.Models;
using Core.Utils;
using UnityEngine;

namespace Core.Handlers
{
    public partial class SlideDownFillStrategy
    {
        private bool TryApplyAnyMove(GridStateContext context)
        {
            var model = context.GridModel;
            var width = model.Width;
            var height = model.Height;

            var movedAny = false;
            var movedByGravity = false;

            movedByGravity |= ApplyVerticalFalls(model, width, height);
            movedByGravity |= ApplyDiagonalSlides(model, width, height);

            movedAny |= movedByGravity;

            if (!movedByGravity)
                movedAny |= ApplySpawns(context, width, height);

            return movedAny;
        }

        private bool ApplyVerticalFalls(IGridModel model, int width, int height)
        {
            var movedAny = false;

            for (int x = 0; x < width; x++)
            {
                for (int y = height - 1; y >= 0; y--)
                {
                    var dst = new Vector2Int(x, y);

                    if (!GridFillCalcUtil.IsEmptyActiveCell(model, dst)) continue;
                    if (!GridFillCalcUtil.CanFallVertically(model, x, y, out var src)) continue;

                    var item = model.GetGridObject(src);
                    if (!item || item.IsStationary) continue;

                    AddStep(item, src, isSpawn: false);

                    model.SetGridObject(src, null);
                    model.SetGridObject(dst, item);

                    AddStep(item, dst, isSpawn: false);
                    movedAny = true;
                }
            }

            return movedAny;
        }

        private bool ApplyDiagonalSlides(IGridModel model, int width, int height)
        {
            var movedAny = false;
            
            _targetStamp.NextPass();
            _sourceStamp.NextPass();

            for (int y = height - 1; y >= 1; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    var targetCoord = new Vector2Int(x, y);

                    if (!GridFillCalcUtil.IsEmptyActiveCell(model, targetCoord)) continue;
                    if (GridFillCalcUtil.CanFallVertically(model, x, y, out _)) continue;

                    // Alternate side preference deterministically
                    var hash = x ^ y ^ GetPassHash();
                    var firstDir = (hash & 1) == 0 ? -1 : 1;
                    var secondDir = -firstDir;

                    if (TryApplySlideCandidate(model, width, targetCoord, firstDir) ||
                        TryApplySlideCandidate(model, width, targetCoord, secondDir))
                    {
                        movedAny = true;
                    }
                }
            }

            return movedAny;
        }

        private bool TryApplySlideCandidate(IGridModel model, int width, Vector2Int targetCoord, int dirX)
        {
            if (!GridFillCalcUtil.TryCollectDiagonalSide(model, targetCoord, dirX, out var candidate))
                return false;

            if (model.GetGridObject(candidate.From) != candidate.Item) return false;
            if (model.GetGridObject(candidate.To)) return false;

            var ti = candidate.To.x + candidate.To.y * width;
            if (!_targetStamp.TryMark(ti)) return false;

            var fi = candidate.From.x + candidate.From.y * width;
            if (!_sourceStamp.TryMark(fi)) return false;

            AddStep(candidate.Item, candidate.From, isSpawn: false);

            model.SetGridObject(candidate.From, null);
            model.SetGridObject(candidate.To, candidate.Item);

            AddStep(candidate.Item, candidate.To, isSpawn: false);
            return true;
        }

        private bool ApplySpawns(GridStateContext context, int width, int height)
        {
            var movedAny = false;

            for (int x = 0; x < width; x++)
            {
                if (SpawnTopOpenSegment(context, x, height))
                    movedAny = true;
            }

            return movedAny;
        }

        // Simple hash for deterministic alternation
        private int _passCounter;
        private int GetPassHash() => _passCounter++;
    }
}
