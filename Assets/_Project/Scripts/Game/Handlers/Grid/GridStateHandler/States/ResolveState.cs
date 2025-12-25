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

        private struct CellResolveData
        {
            public bool Remove;
            public int Damage;
            public DamageSource Source;

            public int ObstacleDamage;
            public DamageSource ObstacleSource;

            public void MarkRemove(DamageSource source)
            {
                Remove = true;
                Source |= source;
            }

            public void AddDamage(int damage, DamageSource source)
            {
                Remove = true;
                Damage = Mathf.Max(Damage, damage);
                Source |= source;
            }

            public void AddObstacleOnlyDamage(int damage, DamageSource source)
            {
                ObstacleDamage = Mathf.Max(ObstacleDamage, damage);
                ObstacleSource |= source;
            }
        }
        
        private struct BoosterSpawn
        {
            public readonly bool HasSpawn;
            public readonly Vector2Int Pos;
            public readonly BoosterType Type;
            public readonly List<Vector2Int> GroupCells;

            public BoosterSpawn(bool hasSpawn, Vector2Int pos, BoosterType type, List<Vector2Int> groupCells)
            {
                HasSpawn = hasSpawn;
                Pos = pos;
                Type = type;
                GroupCells = groupCells;
            }
        }

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

            var cells = new CellResolveData[width, height];
            var spawns = new List<BoosterSpawn>(8);

            if (hasMatch)
            {
                ResolveRegularMatches(typeGrid, matchMask, cells, width, height, spawns);
            }

            var hasEffects = Context.PendingEffects != null && Context.PendingEffects.Count > 0;
            if (hasEffects)
            {
                ApplyPendingEffects(cells, width, height);
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

                    await PlayMatchMergeAnimation(s);
                    SpawnBooster(s.Pos, s.Type);
                }
            }

            ApplyResolveData(cells, width, height);
            RequestExit();
        }

        private void ResolveRegularMatches(GridObjectTypeData[,] typeGrid, bool[,] matchMask, CellResolveData[,] cells, int width, int height, List<BoosterSpawn> spawns)
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
                    if (group.Count == 0) continue;

                    var spawn = DecideBoosterForGroup(typeGrid, group, width, height, id);

                    for (int i = 0; i < group.Count; i++)
                    {
                        var c = group[i];

                        ref var cell = ref cells[c.x, c.y];
                        cell.AddDamage(1, DamageSource.Item);

                        AddNeighborObstacleDamage(cells, width, height, c);
                    }

                    if (spawn.HasSpawn)
                    {
                        spawns.Add(spawn);

                        for (int i = 0; i < group.Count; i++)
                        {
                            var c = group[i];

                            ref var cell = ref cells[c.x, c.y];
                            cell.Remove = false;
                            cell.Source &= ~DamageSource.Item;
                        }
                    }
                }
            }
        }

        private static List<Vector2Int> CollectGroup(bool[,] matchMask, bool[,] visited, GridObjectTypeData[,] typeGrid, int width, int height, Vector2Int start, int id)
        {
            var result = new List<Vector2Int>(16);
            var q = new Queue<Vector2Int>(16);

            visited[start.x, start.y] = true;
            q.Enqueue(start);

            while (q.Count > 0)
            {
                var p = q.Dequeue();
                result.Add(p);

                TryEnqueue(new Vector2Int(p.x - 1, p.y));
                TryEnqueue(new Vector2Int(p.x + 1, p.y));
                TryEnqueue(new Vector2Int(p.x, p.y - 1));
                TryEnqueue(new Vector2Int(p.x, p.y + 1));
            }

            return result;

            void TryEnqueue(Vector2Int n)
            {
                if (n.x < 0 || n.y < 0 || n.x >= width || n.y >= height) return;
                if (!matchMask[n.x, n.y]) return;
                if (visited[n.x, n.y]) return;

                var d = typeGrid[n.x, n.y];
                if (!GridMatchDetectUtil.IsRegularItem(d)) return;
                if (d.TypeId != id) return;

                visited[n.x, n.y] = true;
                q.Enqueue(n);
            }
        }

        private BoosterSpawn DecideBoosterForGroup(GridObjectTypeData[,] grid, List<Vector2Int> cells, int width, int height, int id)
        {
            var bestPriority = 0;
            var bestPos = new Vector2Int(-1, -1);
            var bestType = BoosterType.None;

            for (int i = 0; i < cells.Count; i++)
            {
                var c = cells[i];

                GridMatchDetectUtil.GetLineLengthsAt(grid, c.x, c.y, width, height, id, assumeCenterIsId: true, out var h, out var v);

                if (h >= 5 || v >= 5)
                {
                    var rocket = v >= h ? BoosterType.RocketHorizontal : BoosterType.RocketVertical;

                    TrySet(5, c, rocket);
                    continue;
                }

                if (h >= 3 && v >= 3)
                {
                    TrySet(4, c, BoosterType.Bomb);
                    continue;
                }

                if (GridMatchDetectUtil.Has2x2Square(grid, c.x, c.y, width, height, id, assumeCenterIsId: true))
                {
                    TrySet(3, c, BoosterType.Bomb);
                    continue;
                }

                if (h >= 4 || v >= 4)
                {
                    var rocket = v >= h
                        ? BoosterType.RocketHorizontal
                        : BoosterType.RocketVertical;

                    TrySet(2, c, rocket);
                }
            }

            if (Context.IsForcedBoosterSpawnPos)
            {
                var forced = Context.ForcedBoosterSpawnPos;

                for (int i = 0; i < cells.Count; i++)
                {
                    if (cells[i] == forced)
                    {
                        Context.IsForcedBoosterSpawnPos = false;
                        bestPos = forced;
                        break;
                    }
                }
            }

            return bestPriority == 0 ? default : new BoosterSpawn(true, bestPos, bestType, cells);

            void TrySet(int priority, Vector2Int pos, BoosterType type)
            {
                if (priority <= bestPriority) return;

                bestPriority = priority;
                bestPos = pos;
                bestType = type;
            }
        }

        private static void AddNeighborObstacleDamage(CellResolveData[,] cells, int width, int height, Vector2Int c)
        {
            MarkNeighbour(c.x - 1, c.y);
            MarkNeighbour(c.x + 1, c.y);
            MarkNeighbour(c.x, c.y - 1);
            MarkNeighbour(c.x, c.y + 1);
            return;

            void MarkNeighbour(int x, int y)
            {
                if (x < 0 || y < 0 || x >= width || y >= height) return;
                ref var cell = ref cells[x, y];
                cell.AddObstacleOnlyDamage(1, DamageSource.Item);
            }
        }

        private async UniTask PlayMatchMergeAnimation(BoosterSpawn spawn)
        {
            var targetWorld = Context.View.GridToWorld(spawn.Pos);

            var tasks = new List<UniTask>(spawn.GroupCells.Count);

            for (int i = 0; i < spawn.GroupCells.Count; i++)
            {
                var c = spawn.GroupCells[i];
                if (c == spawn.Pos) continue;

                var obj = Context.Model.GetGridObject(c);
                if (!obj) continue;

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

        private void ApplyPendingEffects(CellResolveData[,] cells, int width, int height)
        {
            for (int i = 0; i < Context.PendingEffects.Count; i++)
            {
                var pending = Context.PendingEffects[i];

                if (Context.Model.IsInRange(pending.Origin))
                {
                    ref var cell = ref cells[pending.Origin.x, pending.Origin.y];
                    cell.MarkRemove(DamageSource.Booster);
                }

                ApplyEffect(cells, width, height, pending.Origin, pending.BoosterEffect);
            }
        }

        private void ApplyEffect(CellResolveData[,] cells, int width, int height, Vector2Int origin, BoosterEffectBase effect)
        {
            switch (effect)
            {
                case RocketHorizontalEffect rocketH:
                    ApplyRowBandDamage(cells, width, height, origin.y, rocketH.LineCount, rocketH.DamageAmount, DamageSource.Booster);
                    return;

                case RocketVerticalEffect rocketV:
                    ApplyColumnBandDamage(cells, width, height, origin.x, rocketV.LineCount, rocketV.DamageAmount, DamageSource.Booster);
                    return;

                case BombEffect bomb:
                    ApplySquareDamage(cells, width, height, origin, bomb.Radius, bomb.DamageAmount, DamageSource.Booster);
                    return;

                case FullGridRemoveEffect full:
                    ApplyAllDamage(cells, width, height, full.DamageAmount, DamageSource.Booster);
                    return;
            }
        }

        private static void ApplyRowBandDamage(CellResolveData[,] cells, int width, int height, int centerY, int lineCount, int damage, DamageSource source)
        {
            var half = Mathf.Max(0, (lineCount - 1) / 2);

            for (int dy = -half; dy <= half; dy++)
            {
                var y = centerY + dy;
                if (y < 0 || y >= height) continue;

                for (int x = 0; x < width; x++)
                {
                    ref var cell = ref cells[x, y];
                    cell.AddDamage(damage, source);
                }
            }
        }

        private static void ApplyColumnBandDamage(CellResolveData[,] cells, int width, int height, int centerX, int lineCount, int damage, DamageSource source)
        {
            var half = Mathf.Max(0, (lineCount - 1) / 2);

            for (int dx = -half; dx <= half; dx++)
            {
                var x = centerX + dx;
                if (x < 0 || x >= width) continue;

                for (int y = 0; y < height; y++)
                {
                    ref var cell = ref cells[x, y];
                    cell.AddDamage(damage, source);
                }
            }
        }

        private static void ApplySquareDamage(CellResolveData[,] cells, int width, int height, Vector2Int center, int radius, int damage, DamageSource source)
        {
            radius = Mathf.Max(0, radius);

            for (int y = center.y - radius; y <= center.y + radius; y++)
            {
                for (int x = center.x - radius; x <= center.x + radius; x++)
                {
                    if (x < 0 || y < 0 || x >= width || y >= height) continue;

                    ref var cell = ref cells[x, y];
                    cell.AddDamage(damage, source);
                }
            }
        }

        private static void ApplyAllDamage(CellResolveData[,] cells, int width, int height, int damage, DamageSource source)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    ref var cell = ref cells[x, y];
                    cell.AddDamage(damage, source);
                }
            }
        }

        private void ApplyResolveData(CellResolveData[,] cells, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    ref var cell = ref cells[x, y];

                    var pos = new Vector2Int(x, y);
                    if (!Context.Model.IsCellActive(pos)) continue;

                    var obj = Context.Model.GetGridObject(pos);
                    if (!obj) continue;

                    var anyWork = cell.Remove || cell.ObstacleDamage > 0;
                    if (!anyWork) continue;

                    if (obj is IDamageableItem damageable)
                    {
                        if (cell.Remove)
                        {
                            var dmg = Mathf.Max(1, cell.Damage);
                            var src = cell.Source;

                            var result = damageable.TakeDamage(dmg, src);
                            if (result == DamageResult.Destroyed)
                            {
                                Context.Factory.ReleaseItem(obj);
                                Context.Model.SetGridObject(pos, null);
                                continue;
                            }
                        }

                        if (cell.ObstacleDamage > 0)
                        {
                            var dmg = Mathf.Max(1, cell.ObstacleDamage);
                            var src = cell.ObstacleSource;

                            var result = damageable.TakeDamage(dmg, src);
                            if (result == DamageResult.Destroyed)
                            {
                                Context.Factory.ReleaseItem(obj);
                                Context.Model.SetGridObject(pos, null);
                                continue;
                            }
                        }

                        continue;
                    }

                    if (cell.Remove)
                    {
                        Context.Factory.ReleaseItem(obj);
                        Context.Model.SetGridObject(pos, null);
                    }
                }
            }
        }
    }
}