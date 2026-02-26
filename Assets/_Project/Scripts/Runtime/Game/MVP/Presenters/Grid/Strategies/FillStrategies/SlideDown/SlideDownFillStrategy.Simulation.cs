using Game.Grid.Utils;
using Game.Models;
using UnityEngine;

namespace Game.Grid.Strategies
{
    public sealed partial class SlideDownFillStrategy
    {
        /// <summary>
        /// Repeatedly applies gravity passes until the grid is stable.
        ///
        /// Loop invariant:
        ///   - Vertical falls and diagonal slides run first.
        ///   - Spawns only run when gravity moved nothing, because a fresh spawn
        ///     may unlock further diagonal slides on the next iteration (e.g. a
        ///     newly spawned item above an obstacle can diagonal-slide past it).
        ///   - The loop continues as long as any pass (gravity OR spawn) made a
        ///     change, guaranteeing every reachable empty cell is eventually filled.
        /// </summary>
        private void RunSimulation()
        {
            var w = _gridModel.Width;
            var h = _gridModel.Height;

            var movedAny = true;
            
            while (movedAny)
            {
                var movedByGravity = ApplyVerticalFalls(w, h);
                movedByGravity |= ApplyDiagonalSlides(w, h);

                // Only spawn when gravity has fully settled for this pass;
                // otherwise spawned items could appear below still-falling items.
                movedAny = movedByGravity || ApplySpawns(w, h);
            }
        }

        /// <summary>
        /// For every empty active cell (scanned bottom-to-top), checks whether
        /// a non-stationary item exists directly above (within the same active
        /// column segment) and drops it one step.
        /// </summary>
        private bool ApplyVerticalFalls(int width, int height)
        {
            var moved = false;

            for (int x = 0; x < width; x++)
            {
                // Bottom-to-top: items that fall earlier in the column open up
                // space for the items above them.
                for (int y = height - 1; y >= 0; y--)
                {
                    var dst = new Vector2Int(x, y);

                    if (!GridFillCalcUtil.IsEmptyActiveCell(_gridModel, dst)) continue;
                    if (!GridFillCalcUtil.CanFallVertically(_gridModel, x, y, out var src)) continue;

                    var item = _gridModel.GetGridObject(src);
                    if (!item || item.IsStationary) continue;

                    AddStep(item, src, isSpawn: false);

                    _gridModel.SetGridObject(src, null);
                    _gridModel.SetGridObject(dst, item);

                    AddStep(item, dst, isSpawn: false);
                    moved = true;
                }
            }

            return moved;
        }

        /// <summary>
        /// For every empty active cell that cannot be filled by a vertical fall,
        /// attempts to pull a non-stationary item from one diagonal neighbor.
        /// Direction preference alternates per cell to avoid systematic bias.
        /// </summary>
        private bool ApplyDiagonalSlides(int width, int height)
        {
            var moved = false;

            // Advance stamps so this pass's deduplication is independent of all
            // previous passes (including from prior Execute() calls).
            AdvanceSlideStamps();

            // Scan bottom-to-top, skipping row 0 (nothing can slide into row 0
            // from below — row index 0 is the topmost row in this coordinate
            // system; slides come from row y-1 into row y).
            for (int y = height - 1; y >= 1; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    var target = new Vector2Int(x, y);

                    if (!GridFillCalcUtil.IsEmptyActiveCell(_gridModel, target)) continue;

                    // Skip cells that can be satisfied by straight vertical fall;
                    // diagonal slides are only a fallback.
                    if (GridFillCalcUtil.CanFallVertically(_gridModel, x, y, out _)) continue;

                    // Deterministic but alternating left/right preference.
                    var preferLeft = ((x ^ y ^ _usedTargetStampId) & 1) == 0;
                    var dir1 = preferLeft ? -1 : +1;
                    var dir2 = -dir1;

                    if (TryApplySlide(_gridModel, width, target, dir1) || TryApplySlide(_gridModel, width, target, dir2))
                    {
                        moved = true;
                    }
                }
            }

            return moved;
        }

        /// <summary>
        /// Attempts to slide the item at <c>target + (dirX, -1)</c> into
        /// <paramref name="target"/>.  Uses per-cell stamps to ensure each cell
        /// is used at most once per diagonal pass (prevents double-moves).
        /// </summary>
        private bool TryApplySlide(IGridModel model, int width, Vector2Int target, int dirX)
        {
            if (!GridFillCalcUtil.TryCollectDiagonalSide(model, target, dirX, out var candidate)) return false;

            // Validate model still matches candidate (could have shifted since
            // TryCollectDiagonalSide ran).
            if (model.GetGridObject(candidate.From) != candidate.Item) return false;
            if (model.GetGridObject(candidate.To)) return false;

            // Dedup: each target cell may only be filled once per pass.
            var ti = candidate.To.x + candidate.To.y * width;
            if (_usedTargetStamp[ti] == _usedTargetStampId) return false;
            _usedTargetStamp[ti] = _usedTargetStampId;

            // Dedup: each source cell may only be emptied once per pass.
            var fi = candidate.From.x + candidate.From.y * width;
            if (_usedSourceStamp[fi] == _usedSourceStampId) return false;
            _usedSourceStamp[fi] = _usedSourceStampId;

            AddStep(candidate.Item, candidate.From, isSpawn: false);

            model.SetGridObject(candidate.From, null);
            model.SetGridObject(candidate.To,   candidate.Item);

            AddStep(candidate.Item, candidate.To, isSpawn: false);
            return true;
        }

        private bool ApplySpawns(int width, int height)
        {
            var spawnedAny = false;
            for (int x = 0; x < width; x++)
                spawnedAny |= SpawnColumn(x, height);
            return spawnedAny;
        }
    }
}