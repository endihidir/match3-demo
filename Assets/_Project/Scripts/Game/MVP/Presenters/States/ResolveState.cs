using System.Collections.Generic;
using Core.Item;
using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class ResolveState : StateBase<GridStateContext>
    {
        public override bool NeedsExitTime => true;

        private const float MergeMoveDuration = 0.1f;
        private const Ease MergeEase = Ease.InOutQuad;
        private readonly IMatchResolveHandler _matchResolveHandler;

        public ResolveState(IMatchResolveHandler matchResolveHandler) => _matchResolveHandler = matchResolveHandler;
        protected override void OnInit() => _matchResolveHandler.Initialize(Context);
        protected override void OnEnter()
        {
            Context.RefillResolveRequested = false;
            
            ResolveAsync().Forget();
        }

        private async UniTask ResolveAsync()
        {
            var width = Context.Model.Width;
            var height = Context.Model.Height;

            var typeGrid = Context.Model.BuildTypeDataGrid();
            var matchMask = GridMatchDetectUtil.BuildMatchMask_ScanBased(typeGrid, width, height, out var hasMatch);

            var cells = new CellResolveData[width, height];
            var spawns = new List<BoosterSpawnResult>(8);

            if (hasMatch)
            {
                ResolveMatches(typeGrid, matchMask, cells, width, height, spawns);
            }

            var hasEffects = Context.PendingEffects is { Count: > 0 };
            
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
                    var spawnResult = spawns[i];
                    if (!spawnResult.HasSpawn) continue;

                    await PlayMatchMergeAnimation(spawnResult);
                    SpawnBooster(spawnResult.Pos, spawnResult.Type);
                }
            }

            ApplyResolveData(cells, width, height);
            RequestExit();
        }

        private void ResolveMatches(GridObjectType[,] typeGrid, bool[,] matchMask, CellResolveData[,] cells, int width, int height, List<BoosterSpawnResult> spawns)
        {
            var visited = new bool[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!matchMask[x, y]) continue;
                    if (visited[x, y]) continue;

                    var startData = typeGrid[x, y];

                    var group = ResolveMarkHelper.CollectGroup(matchMask, visited, typeGrid, width, height, new Vector2Int(x, y), startData.TypeId);
                    if (group.Count == 0) continue;

                    _matchResolveHandler.Handle(Context, typeGrid, group, width, height, cells, spawns);
                }
            }
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

                BoosterEffectApplyHelper.ApplyEffect(cells, width, height, pending.Origin, pending.BoosterAction);
            }
        }

        private async UniTask PlayMatchMergeAnimation(BoosterSpawnResult spawnResult)
        {
            var targetWorld = Context.View.GridToWorld(spawnResult.Pos);

            var tasks = new List<UniTask>(spawnResult.GroupCells.Count);

            for (int i = 0; i < spawnResult.GroupCells.Count; i++)
            {
                var c = spawnResult.GroupCells[i];
                if (c == spawnResult.Pos) continue;

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

            for (int i = 0; i < spawnResult.GroupCells.Count; i++)
            {
                var c = spawnResult.GroupCells[i];
                if (c == spawnResult.Pos) continue;

                var obj = Context.Model.GetGridObject(c);
                if (!obj) continue;

                Context.Factory.ReleaseItem(obj);
                Context.Model.SetGridObject(c, null);
            }

            var targetObj = Context.Model.GetGridObject(spawnResult.Pos);
            if (targetObj)
            {
                Context.Factory.ReleaseItem(targetObj);
                Context.Model.SetGridObject(spawnResult.Pos, null);
            }
        }

        private void SpawnBooster(Vector2Int pos, BoosterType type)
        {
            var booster = Context.Factory.GetItem<BoosterObject>(GridItemKind.Booster, (int)type);
            Context.Model.SetGridObject(pos, booster);

            booster.SetPosition(Context.View.GridToWorld(pos));
            booster.SetSpriteSize(Context.View.GetCellSize());
            booster.SetParent(Context.View.GridObjectsParent);
        }

        private void ApplyResolveData(CellResolveData[,] cells, int width, int height)
        { 
           var triggered = new bool[width, height];
           var effects = new Queue<PendingEffect>(8);

           var loopAgain = false;

           do
           {
               loopAgain = false;

               for (int y = 0; y < height; y++)
               {
                   for (int x = 0; x < width; x++)
                   {
                       if (!TryGetActiveObject(x, y, out var pos, out var obj)) continue;

                       ref var cell = ref cells[x, y];

                       if (!HasAnyWork(cell)) continue;

                       if (obj is IDamageableItem damageable)
                       {
                           ProcessDamageableCell(x, y, pos, obj, damageable, ref cell, ref loopAgain);
                           continue;
                       }

                       ProcessNonDamageableCell(x, y, pos, obj, ref cell, ref loopAgain);
                   }
               }
           }
           while (loopAgain);
            
           return;

           bool TryGetActiveObject(int x, int y, out Vector2Int pos, out BaseGridObject obj)
           {
               pos = new Vector2Int(x, y);

               if (!Context.Model.IsCellActive(pos))
               {
                   obj = null;
                   return false;
               }

               obj = Context.Model.GetGridObject(pos);
               return obj;
           }

           bool HasAnyWork(in CellResolveData cell) => cell.Remove || cell.ObstacleDamage > 0;

           void ProcessDamageableCell(int x, int y, Vector2Int pos, BaseGridObject obj, IDamageableItem damageable, ref CellResolveData cell, ref bool loopAgainFlag)
           {
               if (cell.Remove)
               {
                   EnqueueTriggerIfNeeded(x, y, pos, obj);
                   FlushEffects(ref loopAgainFlag);
                   DestroyAt(pos, obj);
               }

               if (cell.ObstacleDamage > 0)
               {
                   if (ApplyDamage(damageable, Mathf.Max(1, cell.ObstacleDamage), cell.ObstacleSource))
                   {
                        DestroyAt(pos, obj);
                   }
               }
           }

           void ProcessNonDamageableCell(int x, int y, Vector2Int pos, BaseGridObject obj, ref CellResolveData cell, ref bool loopAgainFlag)
           {
               if (!cell.Remove) return;

               EnqueueTriggerIfNeeded(x, y, pos, obj);
               FlushEffects(ref loopAgainFlag);
               DestroyAt(pos, obj);
           }

           bool ApplyDamage(IDamageableItem damageable, int dmg, DamageSource src)
           {
               return damageable.TakeDamage(dmg, src) == DamageResult.Destroyed;
           }

           void EnqueueTriggerIfNeeded(int x, int y, Vector2Int pos, BaseGridObject obj)
           {
               if (triggered[x, y]) return;

               if (obj is ITriggerEffectSource trigger && trigger.TryBuildEffect(pos, out var effect))
               {
                   effects.Enqueue(effect);
                   triggered[x, y] = true;
               }
           }

           void FlushEffects(ref bool loopAgainFlag)
           {
               while (effects.Count > 0)
               {
                   var pendingEffect = effects.Dequeue();
                   BoosterEffectApplyHelper.ApplyEffect(cells, width, height, pendingEffect.Origin, pendingEffect.BoosterAction);
                   loopAgainFlag = true;
               }
           }

           void DestroyAt(Vector2Int pos, BaseGridObject obj)
           {
               Context.Factory.ReleaseItem(obj);
               Context.Model.SetGridObject(pos, null);
           }
        }
    }
}