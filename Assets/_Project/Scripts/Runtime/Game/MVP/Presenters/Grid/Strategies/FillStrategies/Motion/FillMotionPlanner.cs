using System;
using System.Collections.Generic;
using Core.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Configs;
using Game.Grid.Item;
using Game.Grid.Strategies.Data;
using Game.Models;
using Game.Views;
using UnityEngine;

namespace Game.Grid.Strategies
{
    public sealed class FillMotionPlanner : IFillMotionPlanner
    {
        private const float TimeEpsilon = 0.0001f;
        private const float SpeedEpsilon = 0.001f;
        private const float LengthEpsilon = 0.0001f;
        private const float MinAcceleration = 0.01f;

        private readonly IGridModel _gridModel;
        private readonly IGridView _gridView;
        private readonly GridObjectAnimationConfigSO _config;

        private readonly Dictionary<BaseGridObject, FillMotionPlan> _plans = new(128);
        private readonly Stack<FillMotionPlan> _planPool = new(128);
        private readonly List<BaseGridObject> _finishedItems = new(64);
        private readonly List<FillMotionPlan> _touchedPlans = new(128);
        private readonly List<FillSpawnRequest> _spawnRequests = new(64);
        private readonly FillSpawnRequestComparer _spawnRequestComparer = new();
        private readonly List<Vector2Int> _path = new(32);
        private readonly List<FillMotionStep> _pendingSteps = new(32);
        private readonly List<float> _offsets = new(32);
        private readonly List<float> _speeds = new(32);
        private readonly List<UniTask> _tasks = new(128);

        private float[] _freeAt = Array.Empty<float>();
        private float[] _settledAt = Array.Empty<float>();
        private float[] _passedAt = Array.Empty<float>();
        private bool[] _incomingFlows = Array.Empty<bool>();
        private int[] _topRows = Array.Empty<int>();
        private int[] _spawnBaseRows = Array.Empty<int>();
        private float[] _staggerStarts = Array.Empty<float>();
        private bool[] _topFlows = Array.Empty<bool>();

        private Vector3[] _points = new Vector3[32];
        private float[] _durations = new float[32];
        private float[] _waits = new float[32];
        private float[] _easeSlopes = new float[32];
        private float[] _segmentStartSpeeds = new float[32];
        private float[] _segmentEndSpeeds = new float[32];
        private float[] _lengths = new float[32];
        private Vector2Int[] _directions = new Vector2Int[32];
        private bool[] _cruises = new bool[32];

        private int _segmentCount;
        private float _segmentEndTime;
        private int _width;
        private int _height;
        private int _rowOffset;
        private int _rowSpan;
        private float _now;

        private float Acceleration => Mathf.Max(_config.FallAcceleration, MinAcceleration);

        public FillMotionPlanner(IGridModel gridModel, IGridView gridView, GridConfigContainerSO gridConfigContainer)
        {
            _gridModel = gridModel;
            _gridView = gridView;
            _config = gridConfigContainer.AnimationConfig;

            if (!_config)
                EditorLogger.LogError("[FillMotionPlanner] AnimationConfig is not assigned in GridConfigContainer.");
        }

        public void Begin()
        {
            _now = _config.UseUnscaledTime ? Time.unscaledTime : Time.time;
            _width = _gridModel.Width;
            _height = _gridModel.Height;

            _touchedPlans.Clear();
            _spawnRequests.Clear();

            ReleaseFinishedPlans();
            PrepareColumns();
            ResetTables(CalculateRowOffset());

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    var coord = new Vector2Int(x, y);
                    var item = _gridModel.GetGridObject(coord);

                    if (!item) continue;

                    if (_plans.TryGetValue(item, out var plan))
                        ApplyInFlightPlan(plan, coord);

                    SetFreeAt(coord, float.PositiveInfinity);
                }
            }

            PrepareFlows();

            for (int x = 0; x < _width; x++)
                _spawnBaseRows[x] = Mathf.Min(_topRows[x], _spawnBaseRows[x]) - 1;
        }

        public void RecordMove(BaseGridObject item, Vector2Int from, Vector2Int to)
        {
            if (!item || from == to) return;

            FlushSpawnRequests();
            BuildPath(from, to);
            PlanPath(GetOrCreatePlan(item, from));
        }

        public void RecordSpawn(BaseGridObject item, int column, int stackIndex, Vector2Int target)
        {
            if (!item) return;

            _spawnRequests.Add(new FillSpawnRequest(item, column, stackIndex, target));
        }

        public UniTask Commit()
        {
            FlushSpawnRequests();

            _tasks.Clear();

            for (int i = 0; i < _touchedPlans.Count; i++)
            {
                var plan = _touchedPlans[i];
                var tween = PlayPlan(plan);

                plan.Version = plan.Item.Animation.PlacementVersion;
                plan.IsTouched = false;

                if (tween != null)
                    _tasks.Add(tween.ToUniTask());
            }

            _touchedPlans.Clear();

            return _tasks.Count > 0 ? UniTask.WhenAll(_tasks) : UniTask.CompletedTask;
        }

        private void ReleaseFinishedPlans()
        {
            _finishedItems.Clear();

            foreach (var pair in _plans)
            {
                if (!IsPlanAlive(pair.Key, pair.Value))
                    _finishedItems.Add(pair.Key);
            }

            for (int i = 0; i < _finishedItems.Count; i++)
            {
                var item = _finishedItems[i];
                _planPool.Push(_plans[item]);
                _plans.Remove(item);
            }
        }

        private bool IsPlanAlive(BaseGridObject item, FillMotionPlan plan)
        {
            if (!item) return false;
            if (plan.Version != item.Animation.PlacementVersion) return false;
            if (plan.EndTime <= _now) return false;

            return _gridModel.GetGridObject(item.Coord) == item;
        }

        private void PrepareColumns()
        {
            if (_topRows.Length < _width)
            {
                _topRows = new int[_width];
                _spawnBaseRows = new int[_width];
                _staggerStarts = new float[_width];
                _topFlows = new bool[_width];
            }

            if (_incomingFlows.Length < _width * _height)
                _incomingFlows = new bool[_width * _height];

            for (int x = 0; x < _width; x++)
            {
                _topRows[x] = _height;
                _spawnBaseRows[x] = int.MaxValue;
                _staggerStarts[x] = float.PositiveInfinity;

                for (int y = 0; y < _height; y++)
                {
                    if (!_gridModel.IsCellActive(new Vector2Int(x, y))) continue;

                    _topRows[x] = y;
                    break;
                }
            }
        }

        private void PrepareFlows()
        {
            for (int x = 0; x < _width; x++)
            {
                var hasFlow = false;
                var isBlocked = false;

                _topFlows[x] = false;

                for (int y = 0; y < _height; y++)
                {
                    var coord = new Vector2Int(x, y);
                    var index = GetBoardIndex(coord);
                    var item = _gridModel.GetGridObject(coord);

                    _incomingFlows[index] = false;

                    if (!item) continue;

                    if (_plans.ContainsKey(item))
                    {
                        hasFlow = true;

                        if (!isBlocked)
                            _topFlows[x] = true;
                    }
                    else if (item.IsStationary)
                    {
                        hasFlow = false;
                        isBlocked = true;
                    }
                    else
                    {
                        _incomingFlows[index] = hasFlow;
                    }
                }
            }
        }

        private bool HasIncomingFlow(Vector2Int cell)
        {
            if (cell.x < 0 || cell.x >= _width || cell.y < 0 || cell.y >= _height) return false;

            return _incomingFlows[GetBoardIndex(cell)];
        }

        private int GetBoardIndex(Vector2Int cell) => cell.x * _height + cell.y;

        private int CalculateRowOffset()
        {
            var minRow = 0;

            foreach (var pair in _plans)
            {
                var steps = pair.Value.Steps;

                for (int i = 0; i < steps.Count; i++)
                    minRow = Mathf.Min(minRow, Mathf.Min(steps[i].From.y, steps[i].To.y));
            }

            return _height - minRow;
        }

        private void ApplyInFlightPlan(FillMotionPlan plan, Vector2Int coord)
        {
            TrimFinishedSteps(plan);

            var steps = plan.Steps;

            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];

                if (step.ReleaseTime > GetFreeAt(step.From))
                    SetFreeAt(step.From, step.ReleaseTime);

                if (step.EndTime > GetPassedAt(step.To))
                    SetPassedAt(step.To, step.EndTime);

                if (step.From.x != step.To.x)
                    ExtendCornerReservation(step.To, step.EndTime);
            }

            SetSettledAt(coord, plan.EndTime);

            plan.ReadyTime = plan.EndTime;
            plan.ReadySpeed = steps[steps.Count - 1].EndSpeed;
            plan.NewStepStart = steps.Count;
            plan.IsTouched = false;

            RegisterVisualCell(EvaluateCell(plan));
        }

        private void TrimFinishedSteps(FillMotionPlan plan)
        {
            var steps = plan.Steps;
            var count = 0;

            while (count < steps.Count - 1 && steps[count].EndTime <= _now)
                count++;

            if (count > 0)
                steps.RemoveRange(0, count);
        }

        private Vector2 EvaluateCell(FillMotionPlan plan)
        {
            var steps = plan.Steps;

            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];

                if (_now < step.StartTime) return step.From;
                if (_now >= step.EndTime) continue;

                var distance = GetTravelDistance(step.StartSpeed, _now - step.StartTime);
                var progress = step.Length > 0f ? Mathf.Clamp01(distance / step.Length) : 1f;

                return Vector2.Lerp(step.From, step.To, progress);
            }

            return steps[steps.Count - 1].To;
        }

        private void RegisterVisualCell(Vector2 cell)
        {
            var row = Mathf.FloorToInt(cell.y);
            var left = Mathf.FloorToInt(cell.x);
            var right = Mathf.CeilToInt(cell.x);

            LowerSpawnBase(left, row);

            if (right != left)
                LowerSpawnBase(right, row);
        }

        private void LowerSpawnBase(int column, int row)
        {
            if (column < 0 || column >= _width) return;
            if (row >= _topRows[column]) return;

            _spawnBaseRows[column] = Mathf.Min(_spawnBaseRows[column], row);
        }

        private void FlushSpawnRequests()
        {
            if (_spawnRequests.Count == 0) return;

            for (int i = 0; i < _spawnRequests.Count; i++)
            {
                var request = _spawnRequests[i];
                var spawnCell = GetSpawnCell(request);
                var startDelay = _topFlows[request.Column] ? 0f : _config.FallStartDelay;

                SetFreeAt(spawnCell, float.PositiveInfinity);
                request.Item.SetPosition(_gridView.GridToWorld(spawnCell));
                CreatePlan(request.Item, _now + startDelay);
            }

            _spawnRequests.Sort(_spawnRequestComparer);

            for (int i = 0; i < _spawnRequests.Count; i++)
            {
                var request = _spawnRequests[i];

                BuildPath(GetSpawnCell(request), request.Target);
                PlanPath(_plans[request.Item]);
            }

            _spawnRequests.Clear();
        }

        private Vector2Int GetSpawnCell(in FillSpawnRequest request)
        {
            return new Vector2Int(request.Column, _spawnBaseRows[request.Column] - request.StackIndex);
        }

        private FillMotionPlan GetOrCreatePlan(BaseGridObject item, Vector2Int from)
        {
            if (_plans.TryGetValue(item, out var plan)) return plan;

            var startDelay = HasIncomingFlow(from) ? 0f : _config.FallStartDelay;

            return CreatePlan(item, _now + startDelay);
        }

        private FillMotionPlan CreatePlan(BaseGridObject item, float readyTime)
        {
            if (!_plans.TryGetValue(item, out var plan))
            {
                plan = _planPool.Count > 0 ? _planPool.Pop() : new FillMotionPlan();
                plan.IsTouched = false;
                _plans[item] = plan;
            }

            plan.Reset(item, readyTime);
            return plan;
        }

        private void BuildPath(Vector2Int from, Vector2Int to)
        {
            _path.Clear();
            _path.Add(from);

            if (from.x != to.x)
            {
                _path.Add(to);
                return;
            }

            var direction = to.y > from.y ? 1 : -1;

            for (int y = from.y + direction; y != to.y + direction; y += direction)
                _path.Add(new Vector2Int(from.x, y));
        }

        private void PlanPath(FillMotionPlan plan)
        {
            var stepCount = _path.Count - 1;

            if (stepCount <= 0) return;

            var minDeparture = GetMinDeparture();
            var isFirstMove = plan.Steps.Count == 0;
            var stagger = 0f;
            bool isPlanned;

            if (plan.ReadySpeed > 0f && plan.ReadyTime >= minDeparture - TimeEpsilon)
            {
                isPlanned = TryPlanMoving(plan.ReadyTime, plan.ReadySpeed);
            }
            else
            {
                isPlanned = TryPlanFromRest(Mathf.Max(plan.ReadyTime, minDeparture));

                if (isPlanned)
                    stagger = GetStagger(isFirstMove);
            }

            if (!isPlanned)
            {
                EditorLogger.LogError($"[FillMotionPlanner] Path is blocked for {plan.Item} from {_path[0]} to {_path[stepCount]}");
                BuildRestArrivals();
                FillRestSteps(plan.ReadyTime);
            }

            if (stagger > 0f)
                _pendingSteps[0] = _pendingSteps[0].WithReleaseTime(_pendingSteps[0].EndTime + stagger);

            for (int i = 0; i < _pendingSteps.Count; i++)
            {
                var step = _pendingSteps[i];

                plan.Steps.Add(step);
                SetFreeAt(step.From, step.ReleaseTime);

                if (step.EndTime > GetPassedAt(step.To))
                    SetPassedAt(step.To, step.EndTime);
            }

            var last = _pendingSteps[_pendingSteps.Count - 1];

            SetFreeAt(last.To, float.PositiveInfinity);
            SetSettledAt(last.To, last.EndTime);

            if (IsDiagonalHop())
                ExtendCornerReservation(last.To, last.EndTime);

            plan.ReadyTime = last.EndTime;
            plan.ReadySpeed = last.EndSpeed;

            MarkTouched(plan);
        }

        private bool TryPlanMoving(float time, float speed)
        {
            _pendingSteps.Clear();

            for (int i = 1; i < _path.Count; i++)
            {
                var length = GetStepLength(_path[i - 1], _path[i]);
                var freeAt = GetFreeAt(_path[i]);
                var travelTime = GetTravelTime(speed, length);

                if (time + travelTime >= freeAt - TimeEpsilon)
                {
                    AddPendingStep(time, time + travelTime, speed, GetTravelSpeed(speed, length));
                }
                else
                {
                    if (float.IsPositiveInfinity(freeAt)) return false;

                    var duration = freeAt - time;
                    var restTime = GetTravelTime(0f, length);

                    if (duration <= restTime + TimeEpsilon)
                    {
                        var entrySpeed = Mathf.Clamp(GetEntrySpeed(length, duration), 0f, speed);
                        AddPendingStep(time, freeAt, entrySpeed, GetTravelSpeed(entrySpeed, length));
                    }
                    else
                    {
                        AddPendingStep(freeAt - restTime, freeAt, 0f, GetTravelSpeed(0f, length));
                    }
                }

                var step = _pendingSteps[^1];

                time = step.EndTime;
                speed = step.EndSpeed;
            }

            return true;
        }

        private bool TryPlanFromRest(float departure)
        {
            BuildRestArrivals();

            for (int i = 1; i < _path.Count; i++)
                departure = Mathf.Max(departure, GetFreeAt(_path[i]) - _offsets[i]);

            if (float.IsInfinity(departure)) return false;

            FillRestSteps(departure);
            return true;
        }

        private void FillRestSteps(float departure)
        {
            _pendingSteps.Clear();

            for (int i = 1; i < _path.Count; i++)
                AddPendingStep(departure + _offsets[i - 1], departure + _offsets[i], _speeds[i - 1], _speeds[i]);
        }

        private void AddPendingStep(float startTime, float endTime, float startSpeed, float endSpeed)
        {
            var from = _path[_pendingSteps.Count];
            var to = _path[_pendingSteps.Count + 1];

            _pendingSteps.Add(new FillMotionStep(from, to, startTime, endTime, endTime, startSpeed, endSpeed, GetStepLength(from, to)));
        }

        private float GetStagger(bool isFirstMove)
        {
            var source = _path[0];

            if (isFirstMove && HasIncomingFlow(source)) return 0f;
            if (source.x < 0 || source.x >= _width) return _config.FallStagger;

            var departure = _pendingSteps[0].StartTime;

            if (departure < _staggerStarts[source.x])
                _staggerStarts[source.x] = departure;

            return departure - _staggerStarts[source.x] < _config.FallStaggerLimit - TimeEpsilon ? _config.FallStagger : 0f;
        }

        private bool IsDiagonalHop() => _path.Count == 2 && _path[0].x != _path[1].x;

        private float GetMinDeparture()
        {
            if (!IsDiagonalHop()) return float.NegativeInfinity;

            var source = _path[0];
            var target = _path[1];

            var above = GetFreeAt(new Vector2Int(target.x, target.y - 1));

            if (float.IsPositiveInfinity(above))
                above = float.NegativeInfinity;

            var support = GetSettledAt(new Vector2Int(source.x, target.y));
            var passed = GetPassedAt(target);

            return Mathf.Max(above, Mathf.Max(support, passed));
        }

        private void ExtendCornerReservation(Vector2Int target, float time)
        {
            var corner = new Vector2Int(target.x, target.y - 1);
            var current = GetFreeAt(corner);

            if (float.IsPositiveInfinity(current) || current >= time) return;

            SetFreeAt(corner, time);
        }

        private void BuildRestArrivals()
        {
            _offsets.Clear();
            _speeds.Clear();
            _offsets.Add(0f);
            _speeds.Add(0f);

            var time = 0f;
            var speed = 0f;

            for (int i = 1; i < _path.Count; i++)
            {
                var length = GetStepLength(_path[i - 1], _path[i]);

                time += GetTravelTime(speed, length);
                speed = GetTravelSpeed(speed, length);

                _offsets.Add(time);
                _speeds.Add(speed);
            }
        }

        private float GetSpeedCap(float startSpeed)
        {
            var cap = _config.FallMaxSpeed > 0f ? _config.FallMaxSpeed : float.PositiveInfinity;
            return Mathf.Max(cap, startSpeed);
        }

        private float GetTravelTime(float startSpeed, float length)
        {
            var acceleration = Acceleration;
            var cap = GetSpeedCap(startSpeed);

            if (startSpeed >= cap - SpeedEpsilon) return length / cap;

            var accelerationLength = (cap * cap - startSpeed * startSpeed) / (2f * acceleration);

            if (length <= accelerationLength)
                return (Mathf.Sqrt(startSpeed * startSpeed + 2f * acceleration * length) - startSpeed) / acceleration;

            return (cap - startSpeed) / acceleration + (length - accelerationLength) / cap;
        }

        private float GetTravelSpeed(float startSpeed, float length)
        {
            return Mathf.Min(GetSpeedCap(startSpeed), Mathf.Sqrt(startSpeed * startSpeed + 2f * Acceleration * length));
        }

        private float GetTravelDistance(float startSpeed, float time)
        {
            var acceleration = Acceleration;
            var cap = GetSpeedCap(startSpeed);

            if (startSpeed >= cap - SpeedEpsilon) return cap * time;

            var accelerationTime = (cap - startSpeed) / acceleration;

            if (time <= accelerationTime)
                return startSpeed * time + .5f * acceleration * time * time;

            return (cap * cap - startSpeed * startSpeed) / (2f * acceleration) + cap * (time - accelerationTime);
        }

        private float GetEntrySpeed(float length, float duration)
        {
            var acceleration = Acceleration;
            var cap = GetSpeedCap(0f);
            var speed = length / duration - .5f * acceleration * duration;

            if (speed + acceleration * duration > cap)
                speed = cap - Mathf.Sqrt(Mathf.Max(0f, 2f * acceleration * (cap * duration - length)));

            return speed;
        }

        private float GetStepLength(Vector2Int from, Vector2Int to)
        {
            return from.x != to.x ? _config.DiagonalStepMultiplier : 1f;
        }

        private static float GetEaseSlope(float startSpeed, float duration, float length)
        {
            return length > 0f ? Mathf.Clamp01(startSpeed * duration / length) : 1f;
        }

        private void MarkTouched(FillMotionPlan plan)
        {
            if (plan.IsTouched) return;

            plan.IsTouched = true;
            _touchedPlans.Add(plan);
        }

        private Tween PlayPlan(FillMotionPlan plan)
        {
            var steps = plan.Steps;
            var first = plan.NewStepStart;

            if (first >= steps.Count) return null;

            EnsureSegmentCapacity((steps.Count - first) * 2);

            _segmentCount = 0;
            _segmentEndTime = steps[first].StartTime;

            for (int i = first; i < steps.Count; i++)
                AddStepSegments(steps[i]);

            for (int i = 0; i < _segmentCount; i++)
                _easeSlopes[i] = _cruises[i] ? 1f : GetEaseSlope(_segmentStartSpeeds[i], _durations[i], _lengths[i]);

            var startDelay = Mathf.Max(0f, steps[first].StartTime - _now);

            return plan.Item.Animation.PlayPlacement(_points, _durations, _waits, _easeSlopes, _segmentCount, startDelay);
        }

        private void AddStepSegments(in FillMotionStep step)
        {
            var target = _gridView.GridToWorld(step.To);
            var direction = step.To - step.From;
            var cap = GetSpeedCap(step.StartSpeed);

            if (step.StartSpeed < cap - SpeedEpsilon && step.EndSpeed >= cap - SpeedEpsilon)
            {
                var acceleration = Acceleration;
                var accelerationLength = (cap * cap - step.StartSpeed * step.StartSpeed) / (2f * acceleration);

                if (accelerationLength < step.Length - LengthEpsilon)
                {
                    var splitTime = Mathf.Min(step.StartTime + (cap - step.StartSpeed) / acceleration, step.EndTime);
                    var splitPoint = Vector3.LerpUnclamped(_gridView.GridToWorld(step.From), target, accelerationLength / step.Length);

                    AddSegment(splitPoint, step.StartTime, splitTime, step.StartSpeed, cap, accelerationLength, direction);
                    AddSegment(target, splitTime, step.EndTime, cap, cap, step.Length - accelerationLength, direction);
                    return;
                }
            }

            AddSegment(target, step.StartTime, step.EndTime, step.StartSpeed, step.EndSpeed, step.Length, direction);
        }

        private void AddSegment(Vector3 point, float startTime, float endTime, float startSpeed, float endSpeed, float length, Vector2Int direction)
        {
            var wait = Mathf.Max(0f, startTime - _segmentEndTime);
            var duration = endTime - startTime;
            var isCruise = Mathf.Abs(endSpeed - startSpeed) <= SpeedEpsilon;

            _segmentEndTime = endTime;

            if (CanMergeSegment(wait, startSpeed, direction, isCruise))
            {
                var last = _segmentCount - 1;

                _points[last] = point;
                _durations[last] += duration;
                _lengths[last] += length;
                _segmentEndSpeeds[last] = endSpeed;
                return;
            }

            _points[_segmentCount] = point;
            _durations[_segmentCount] = duration;
            _waits[_segmentCount] = wait;
            _segmentStartSpeeds[_segmentCount] = startSpeed;
            _segmentEndSpeeds[_segmentCount] = endSpeed;
            _lengths[_segmentCount] = length;
            _directions[_segmentCount] = direction;
            _cruises[_segmentCount] = isCruise;
            _segmentCount++;
        }

        private bool CanMergeSegment(float wait, float startSpeed, Vector2Int direction, bool isCruise)
        {
            if (_segmentCount == 0) return false;
            if (wait > TimeEpsilon) return false;

            var last = _segmentCount - 1;

            if (_directions[last] != direction) return false;
            if (_cruises[last] != isCruise) return false;

            return Mathf.Abs(_segmentEndSpeeds[last] - startSpeed) <= SpeedEpsilon;
        }

        private void EnsureSegmentCapacity(int count)
        {
            if (_points.Length >= count) return;

            var size = Mathf.Max(count, _points.Length * 2);

            _points = new Vector3[size];
            _durations = new float[size];
            _waits = new float[size];
            _easeSlopes = new float[size];
            _segmentStartSpeeds = new float[size];
            _segmentEndSpeeds = new float[size];
            _lengths = new float[size];
            _directions = new Vector2Int[size];
            _cruises = new bool[size];
        }

        private void ResetTables(int rowOffset)
        {
            _rowOffset = rowOffset;
            _rowSpan = _rowOffset + _height;

            var size = _width * _rowSpan;

            if (_freeAt.Length < size)
            {
                _freeAt = new float[size];
                _settledAt = new float[size];
                _passedAt = new float[size];
            }

            for (int i = 0; i < size; i++)
            {
                _freeAt[i] = float.NegativeInfinity;
                _settledAt[i] = float.NegativeInfinity;
                _passedAt[i] = float.NegativeInfinity;
            }
        }

        private void EnsureRow(int row)
        {
            if (row + _rowOffset >= 0) return;

            var rowOffset = Mathf.Max(_rowOffset * 2, _height - row);
            var rowSpan = rowOffset + _height;

            _freeAt = ResizeTable(_freeAt, rowOffset, rowSpan);
            _settledAt = ResizeTable(_settledAt, rowOffset, rowSpan);
            _passedAt = ResizeTable(_passedAt, rowOffset, rowSpan);

            _rowOffset = rowOffset;
            _rowSpan = rowSpan;
        }

        private float[] ResizeTable(float[] source, int rowOffset, int rowSpan)
        {
            var table = new float[_width * rowSpan];

            for (int i = 0; i < table.Length; i++)
                table[i] = float.NegativeInfinity;

            var shift = rowOffset - _rowOffset;

            for (int x = 0; x < _width; x++)
                Array.Copy(source, x * _rowSpan, table, x * rowSpan + shift, _rowSpan);

            return table;
        }

        private int GetIndex(Vector2Int cell)
        {
            if (cell.x < 0 || cell.x >= _width || cell.y >= _height) return -1;

            EnsureRow(cell.y);
            return cell.x * _rowSpan + cell.y + _rowOffset;
        }

        private float GetFreeAt(Vector2Int cell)
        {
            var index = GetIndex(cell);
            return index < 0 ? float.NegativeInfinity : _freeAt[index];
        }

        private void SetFreeAt(Vector2Int cell, float time)
        {
            var index = GetIndex(cell);
            if (index >= 0) _freeAt[index] = time;
        }

        private float GetSettledAt(Vector2Int cell)
        {
            var index = GetIndex(cell);
            return index < 0 ? float.NegativeInfinity : _settledAt[index];
        }

        private void SetSettledAt(Vector2Int cell, float time)
        {
            var index = GetIndex(cell);
            if (index >= 0) _settledAt[index] = time;
        }

        private float GetPassedAt(Vector2Int cell)
        {
            var index = GetIndex(cell);
            return index < 0 ? float.NegativeInfinity : _passedAt[index];
        }

        private void SetPassedAt(Vector2Int cell, float time)
        {
            var index = GetIndex(cell);
            if (index >= 0) _passedAt[index] = time;
        }
    }
}