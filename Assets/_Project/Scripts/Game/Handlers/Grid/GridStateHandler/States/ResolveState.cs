using System.Collections.Generic;
using Core.Config;
using Core.Item;
using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class ResolveState : StateBase<GridContext>
    {
        public override bool NeedsExitTime => true;

        private const float MergeMoveDuration = 0.12f;
        private const Ease MergeEase = Ease.InOutQuad;

        protected override void OnEnter()
        {
            Context.CascadeResolveRequested = false;
            ResolveAsync().Forget();
        }

        private async UniTaskVoid ResolveAsync()
        {
            var width = Context.Model.Width;
            var height = Context.Model.Height;

            var typeGrid = Context.Model.BuildTypeDataGrid();
            var matchMask = GridMatchDetectUtil.BuildMatchMaskFast(typeGrid, width, height, out var hasMatch);

            var removeMask = new bool[width, height];
            var cellDamage = new int[width, height];

            var spawns = new List<BoosterSpawn>(8);

            if (hasMatch)
            {
                ResolveRegularMatches(typeGrid, matchMask, removeMask, cellDamage, width, height, spawns);
            }

            var hasEffects = Context.PendingEffects != null && Context.PendingEffects.Count > 0;
            if (hasEffects)
            {
                ApplyPendingEffects(removeMask, cellDamage, width, height);
                Context.PendingEffects.Clear();
            }

            Context.ResolvedAnyMatch = hasMatch || hasEffects;

            if (!Context.ResolvedAnyMatch)
            {
                RequestExit();
                return;
            }

            if (spawns.Count > 0)
            {
                for (int i = 0; i < spawns.Count; i++)
                {
                    var s = spawns[i];
                    if (!s.HasSpawn) continue;

                    await PlayMatchMergeAnimation(s, width, height);
                    SpawnBooster(s.Pos, s.Type);
                }
            }

            ResolveRemoveMask(removeMask, cellDamage, width, height);

            RequestExit();
        }

        private struct BoosterSpawn
        {
            public bool HasSpawn;
            public Vector2Int Pos;
            public BoosterType Type;
            public List<Vector2Int> GroupCells;
        }

        private void ResolveRegularMatches(GridObjectTypeData[,] typeGrid, bool[,] matchMask, bool[,] removeMask, int[,] cellDamage, int width, int height, List<BoosterSpawn> spawns)
        {
            var visited = new bool[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!matchMask[x, y]) continue;
                    if (visited[x, y]) continue;

                    var startData = typeGrid[x, y];
                    if (!GridMatchDetectUtil.IsRegularItem(startData)) continue;

                    var id = startData.TypeId;

                    var group = CollectGroup(matchMask, visited, typeGrid, width, height, new Vector2Int(x, y), id);

                    var spawn = DecideBoosterForGroup(typeGrid, group, width, height, id);

                    for (int i = 0; i < group.Count; i++)
                    {
                        var c = group[i];
                        removeMask[c.x, c.y] = true;
                        cellDamage[c.x, c.y] = Mathf.Max(cellDamage[c.x, c.y], 1);
                    }

                    if (spawn.HasSpawn)
                    {
                        removeMask[spawn.Pos.x, spawn.Pos.y] = false;
                        cellDamage[spawn.Pos.x, spawn.Pos.y] = 0;
                        spawns.Add(spawn);
                    }
                }
            }
        }

        private static List<Vector2Int> CollectGroup(bool[,] matchMask, bool[,] visited, GridObjectTypeData[,] typeGrid, int width, int height, Vector2Int start, int id)
        {
            var result = new List<Vector2Int>(8);
            var q = new Queue<Vector2Int>();

            visited[start.x, start.y] = true;
            q.Enqueue(start);

            while (q.Count > 0)
            {
                var p = q.Dequeue();
                result.Add(p);

                TryEnqueue(p + Vector2Int.right);
                TryEnqueue(p + Vector2Int.left);
                TryEnqueue(p + Vector2Int.up);
                TryEnqueue(p + Vector2Int.down);

                void TryEnqueue(Vector2Int n)
                {
                    if (n.x < 0 || n.y < 0 || n.x >= width || n.y >= height) return;
                    if (visited[n.x, n.y]) return;
                    if (!matchMask[n.x, n.y]) return;

                    var d = typeGrid[n.x, n.y];
                    if (!GridMatchDetectUtil.IsRegularItem(d)) return;
                    if (d.TypeId != id) return;

                    visited[n.x, n.y] = true;
                    q.Enqueue(n);
                }
            }

            return result;
        }

        private BoosterSpawn DecideBoosterForGroup(GridObjectTypeData[,] grid, List<Vector2Int> cells, int width, int height, int id)
        {
            var bestPriority = 0;
            var bestPos = new Vector2Int(-1, -1);
            var bestType = BoosterType.None;

            for (int i = 0; i < cells.Count; i++)
            {
                var c = cells[i];

                GetLineLengthsAt(grid, c.x, c.y, width, height, id, out var h, out var v);

                if (h >= 5 || v >= 5)
                {
                    TrySet(5, c, BoosterType.Orb);
                    continue;
                }

                if (h >= 3 && v >= 3)
                {
                    TrySet(4, c, BoosterType.Bomb);
                    continue;
                }

                if (GridMatchDetectUtil.Has2x2Square(grid, c.x, c.y, width, height, id, assumeCenterIsId: false))
                {
                    TrySet(3, c, BoosterType.Fly);
                    continue;
                }

                if (h >= 4 || v >= 4)
                {
                    var rocket = v >= h ? BoosterType.RocketHorizontal : BoosterType.RocketVertical;
                    TrySet(2, c, rocket);
                }
            }

            if (bestPriority == 0)
                return default;

            if (bestType == BoosterType.Fly)
                bestType = BoosterType.Bomb;

            if (bestType == BoosterType.Orb)
                bestType = BoosterType.RocketHorizontal;

            return new BoosterSpawn
            {
                HasSpawn = true,
                Pos = bestPos,
                Type = bestType,
                GroupCells = cells
            };

            void TrySet(int priority, Vector2Int pos, BoosterType type)
            {
                if (priority <= bestPriority) return;

                bestPriority = priority;
                bestPos = pos;
                bestType = type;
            }
        }

        private static void GetLineLengthsAt(GridObjectTypeData[,] grid, int x, int y, int width, int height, int id, out int horizontal, out int vertical)
        {
            horizontal = 1 + CountSame(-1, 0) + CountSame(1, 0);
            vertical = 1 + CountSame(0, -1) + CountSame(0, 1);

            int CountSame(int dx, int dy)
            {
                var count = 0;

                var cx = x + dx;
                var cy = y + dy;

                while (cx >= 0 && cx < width && cy >= 0 && cy < height)
                {
                    var data = grid[cx, cy];
                    if (!GridMatchDetectUtil.IsRegularItem(data)) break;
                    if (data.TypeId != id) break;

                    count++;
                    cx += dx;
                    cy += dy;
                }

                return count;
            }
        }

        private async UniTask PlayMatchMergeAnimation(BoosterSpawn spawn, int width, int height)
        {
            var targetWorld = Context.View.GridToWorld(spawn.Pos);

            var tasks = new List<UniTask>(spawn.GroupCells.Count);

            for (int i = 0; i < spawn.GroupCells.Count; i++)
            {
                var c = spawn.GroupCells[i];
                if (c == spawn.Pos) continue;

                var obj = Context.Model.GetGridObject(c);
                if (!obj) continue;

                if (obj.IsShiftInProgress) continue;

                var tween = obj.ItemAnimation.Move(targetWorld, MergeMoveDuration, MergeEase);
                if (tween == null)
                {
                    obj.SetPosition(targetWorld);
                    continue;
                }

                tasks.Add(tween.AsyncWaitForCompletion().AsUniTask());
            }

            if (tasks.Count > 0)
                await UniTask.WhenAll(tasks);

            for (int i = 0; i < spawn.GroupCells.Count; i++)
            {
                var c = spawn.GroupCells[i];
                if (c == spawn.Pos) continue;

                var obj = Context.Model.GetGridObject(c);
                if (!obj) continue;

                Context.Factory.ReleaseItem(obj);
                Context.Model.SetGridObject(c, null);
            }

            var targetObj = Context.Model.GetGridObject(spawn.Pos);
            if (targetObj)
            {
                Context.Factory.ReleaseItem(targetObj);
                Context.Model.SetGridObject(spawn.Pos, null);
            }
        }

        private void SpawnBooster(Vector2Int pos, BoosterType type)
        {
            var booster = Context.Factory.GetItem<BoosterObject>(GridItemKind.Booster, (int)type);
            Context.Model.SetGridObject(pos, booster);

            booster.SetPosition(Context.View.GridToWorld(pos));
            booster.SetSpriteSize(Context.View.GetCellSize());
        }

        private void ApplyPendingEffects(bool[,] removeMask, int[,] cellDamage, int width, int height)
        {
            for (int i = 0; i < Context.PendingEffects.Count; i++)
            {
                var pending = Context.PendingEffects[i];

                if (Context.Model.IsInRange(pending.Origin))
                    removeMask[pending.Origin.x, pending.Origin.y] = true;

                ApplyEffect(removeMask, cellDamage, width, height, pending.Origin, pending.BoosterEffect);
            }
        }

        private void ApplyEffect(bool[,] removeMask, int[,] cellDamage, int width, int height, Vector2Int origin, BoosterEffectBase effect)
        {
            if (effect == null) return;

            if (effect is RocketHorizontalEffect rocketH)
            {
                MarkRowBand(removeMask, width, height, origin.y, rocketH.LineCount);
                ApplyDamage(cellDamage, removeMask, width, height, rocketH.DamageAmount);
                return;
            }

            if (effect is RocketVerticalEffect rocketV)
            {
                MarkColumnBand(removeMask, width, height, origin.x, rocketV.LineCount);
                ApplyDamage(cellDamage, removeMask, width, height, rocketV.DamageAmount);
                return;
            }

            if (effect is BombEffect bomb)
            {
                EditorLogger.Log(bomb.Radius);
                MarkSquare(removeMask, width, height, origin, bomb.Radius);
                ApplyDamage(cellDamage, removeMask, width, height, bomb.DamageAmount);
                return;
            }

            if (effect is FullGridRemoveEffect full)
            {
                MarkAll(removeMask, width, height);
                ApplyDamage(cellDamage, removeMask, width, height, full.DamageAmount);
            }
        }

        private void ResolveRemoveMask(bool[,] removeMask, int[,] cellDamage, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!removeMask[x, y]) continue;

                    var pos = new Vector2Int(x, y);
                    if (!Context.Model.IsCellActive(pos)) continue;

                    var obj = Context.Model.GetGridObject(pos);
                    if (!obj) continue;

                    var damage = Mathf.Max(1, cellDamage[x, y]);

                    if (obj is IDamageableItem damageable)
                    {
                        damageable.TakeDamage(damage, DamageSource.Booster, () =>
                        {
                            Context.Factory.ReleaseItem(obj);
                            Context.Model.SetGridObject(pos, null);
                        });

                        continue;
                    }

                    Context.Factory.ReleaseItem(obj);
                    Context.Model.SetGridObject(pos, null);
                }
            }
        }

        private static void ApplyDamage(int[,] cellDamage, bool[,] mask, int width, int height, int damage)
        {
            if (damage <= 0) return;

            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (mask[x, y])
                    cellDamage[x, y] = Mathf.Max(cellDamage[x, y], damage);
        }

        private static void MarkRowBand(bool[,] mask, int width, int height, int centerY, int lineCount)
        {
            var half = Mathf.Max(0, (lineCount - 1) / 2);

            for (int dy = -half; dy <= half; dy++)
            {
                var y = centerY + dy;
                if (y < 0 || y >= height) continue;

                for (int x = 0; x < width; x++)
                    mask[x, y] = true;
            }
        }

        private static void MarkColumnBand(bool[,] mask, int width, int height, int centerX, int lineCount)
        {
            var half = Mathf.Max(0, (lineCount - 1) / 2);

            for (int dx = -half; dx <= half; dx++)
            {
                var x = centerX + dx;
                if (x < 0 || x >= width) continue;

                for (int y = 0; y < height; y++)
                    mask[x, y] = true;
            }
        }

        private static void MarkSquare(bool[,] mask, int width, int height, Vector2Int center, int radius)
        {
            radius = Mathf.Max(0, radius);

            for (int y = center.y - radius; y <= center.y + radius; y++)
            {
                for (int x = center.x - radius; x <= center.x + radius; x++)
                {
                    if (x < 0 || y < 0 || x >= width || y >= height) continue;
                    mask[x, y] = true;
                }
            }
        }

        private static void MarkAll(bool[,] mask, int width, int height)
        {
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                mask[x, y] = true;
        }
    }
}
