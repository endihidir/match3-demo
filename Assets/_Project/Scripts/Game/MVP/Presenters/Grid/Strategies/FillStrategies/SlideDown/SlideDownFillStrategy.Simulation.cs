using System;
using Game.Grid.Utils;
using Game.Models;
using UnityEngine;

namespace Game.Grid.Strategies
{
    public sealed partial class SlideDownFillStrategy
    {
        // =========================================================
        // Simulation loop
        // =========================================================

        private bool TryApplyAnyMove()
        {
            var height = _gridModel.Height;
            var width = _gridModel.Width;
            
            var movedAny = false;
            var movedByGravity = false;
            
            movedByGravity |= ApplyVerticalFalls(width, height);
            movedByGravity |= ApplyDiagonalSlides(width, height);
            
            movedAny |= movedByGravity;
            
            if (!movedByGravity)
                movedAny |= ApplySpawns(width, height);

            return movedAny;
        }

        private bool ApplyVerticalFalls(int width, int height)
        {
            var movedAny = false;

            for (int x = 0; x < width; x++)
            {
                for (int y = height - 1; y >= 0; y--)
                {
                    var dst = new Vector2Int(x, y);

                    if (!GridFillCalcUtil.IsEmptyActiveCell(_gridModel, dst)) continue;
                    if (!GridFillCalcUtil.CanFallVertically(_gridModel, x, y, out var src)) continue;

                    var item = _gridModel.GetGridObject(src);
                    if (!item || item.IsStationary) continue;

                    AddStep(item, src, false);
                    
                    _gridModel.SetGridObject(src, null);
                    _gridModel.SetGridObject(dst, item);

                    AddStep(item, dst, false);
                    movedAny = true;
                }
            }

            return movedAny;
        }

        private bool ApplyDiagonalSlides(int width, int height)
        {
            var movedAny = false;
            BumpStamps();

            for (int y = height - 1; y >= 1; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    var targetCoord = new Vector2Int(x, y);

                    if (!GridFillCalcUtil.IsEmptyActiveCell(_gridModel, targetCoord)) continue;
                    if (GridFillCalcUtil.CanFallVertically(_gridModel, x, y, out _)) continue;

                    // Alternate side preference deterministically
                    var firstDir = ((x ^ y ^ _usedTargetStampId) & 1) == 0 ? -1 : 1;
                    var secondDir = -firstDir;

                    if (TryApplySlideCandidate(_gridModel, width, targetCoord, firstDir) || 
                        TryApplySlideCandidate(_gridModel, width, targetCoord, secondDir))
                    {
                        //AddStep(movedItem, target, false);
                        movedAny = true;
                    }
                }
            }

            return movedAny;
        }

        private bool ApplySpawns(int width, int height)
        {
            var movedAny = false;

            for (int x = 0; x < width; x++)
            {
                if (SpawnTopOpenSegment(x, height)) 
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