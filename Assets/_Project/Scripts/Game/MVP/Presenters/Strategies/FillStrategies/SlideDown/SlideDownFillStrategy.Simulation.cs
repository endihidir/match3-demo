using System;
using Core.Models;
using Core.Utils;
using UnityEngine;

namespace Core.Handlers
{
    public partial class SlideDownFillStrategy
    {
        // =========================================================
        // Simulation loop
        // =========================================================

        private bool TryApplyAnyMove(GridStateContext context)
        {
            var height = context.Model.Height;
            var width = context.Model.Width;
            
            var movedAny = false;
            var movedByGravity = false;
            
            movedByGravity |= ApplyVerticalFalls(context.Model, width, height);
            movedByGravity |= ApplyDiagonalSlides(context.Model, width, height);
            
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

                    AddStep(item, src, false);
                    
                    model.SetGridObject(src, null);
                    model.SetGridObject(dst, item);

                    AddStep(item, dst, false);
                    movedAny = true;
                }
            }

            return movedAny;
        }

        private bool ApplyDiagonalSlides(IGridModel model, int width, int height)
        {
            var movedAny = false;
            BumpStamps();

            for (int y = height - 1; y >= 1; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    var targetCoord = new Vector2Int(x, y);

                    if (!GridFillCalcUtil.IsEmptyActiveCell(model, targetCoord)) continue;
                    if (GridFillCalcUtil.CanFallVertically(model, x, y, out _)) continue;

                    // Alternate side preference deterministically
                    var firstDir = ((x ^ y ^ _usedTargetStampId) & 1) == 0 ? -1 : 1;
                    var secondDir = -firstDir;

                    if (TryApplySlideCandidate(model, width, targetCoord, firstDir) || 
                        TryApplySlideCandidate(model, width, targetCoord, secondDir))
                    {
                        //AddStep(movedItem, target, false);
                        movedAny = true;
                    }
                }
            }

            return movedAny;
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

        private void BumpStamps()
        {
            _usedTargetStampId++;
            _usedSourceStampId++;

            // Rare overflow guard
            if (_usedTargetStampId == int.MaxValue || _usedSourceStampId == int.MaxValue)
            {
                Array.Clear(_usedTargetStamp, 0, _usedTargetStamp.Length);
                Array.Clear(_usedSourceStamp, 0, _usedSourceStamp.Length);
                _usedTargetStampId = 1;
                _usedSourceStampId = 1;
            }
        }

        private bool TryApplySlideCandidate(IGridModel model, int width, Vector2Int targetCoord, int dirX)
        {
            if (!GridFillCalcUtil.TryCollectDiagonalSide(model, targetCoord, dirX, out var candidate))
                return false;

            if (model.GetGridObject(candidate.From) != candidate.Item) return false;
            if (model.GetGridObject(candidate.To)) return false;

            var ti = candidate.To.x + candidate.To.y * width;
            if (_usedTargetStamp[ti] == _usedTargetStampId) return false;
            _usedTargetStamp[ti] = _usedTargetStampId;

            var fi = candidate.From.x + candidate.From.y * width;
            if (_usedSourceStamp[fi] == _usedSourceStampId) return false;
            _usedSourceStamp[fi] = _usedSourceStampId;

            AddStep(candidate.Item, candidate.From, false);

            model.SetGridObject(candidate.From, null);
            model.SetGridObject(candidate.To, candidate.Item);
            
            AddStep(candidate.Item, candidate.To, false);
            return true;
        }
    }
}
