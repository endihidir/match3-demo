using System;
using System.Collections.Generic;
using Core.Configs;
using Core.Models;
using Core.Views;
using UnityEngine;

namespace Core.Utils
{
    public static class BoosterImpactResolver
    {
        public static ImpactTimeline BuildTimeline(BoosterActionContext action, IGridModel model, IGridView view, float speed)
        {
            var cellSize = view.GetCellSize();

            return action.BoosterAction switch
            {
                RocketHorizontalAction rha => BuildLinearTimeline(action.OriginCoord, rha.LineCount, true, model, cellSize, speed),
                RocketVerticalAction rva => BuildLinearTimeline(action.OriginCoord, rva.LineCount, false, model, cellSize, speed),
                BombAction ba => BuildSquareTimeline(action.OriginCoord, ba.Radius, model),
                _ => new ImpactTimeline()
            };
        }
        
        private static ImpactTimeline BuildLinearTimeline(Vector2Int origin, int lineCount, bool isHorizontal, IGridModel model, float cellSize, float speed)
        {
            var timeline = new ImpactTimeline();
            var offsets = BuildLineOffsets(lineCount);
            var timePerCell = cellSize / speed;

            foreach (var offset in offsets)
            {
                var lineIndex = isHorizontal ? origin.y + offset : origin.x + offset;
                var maxIndex = isHorizontal ? model.Height : model.Width;
                
                if (lineIndex < 0 || lineIndex >= maxIndex) continue;

                var axisMax = isHorizontal ? model.Width : model.Height;
                var originAxis = isHorizontal ? origin.x : origin.y;
                
                for (int i = 1; i < axisMax - originAxis; i++)
                {
                    var coord = isHorizontal ? new Vector2Int(origin.x + i, lineIndex) : new Vector2Int(lineIndex, origin.y + i);
                    timeline.Add(coord, i * timePerCell);
                }
                
                for (int i = 1; i <= originAxis; i++)
                {
                    var coord = isHorizontal ? new Vector2Int(origin.x - i, lineIndex) : new Vector2Int(lineIndex, origin.y - i);
                    timeline.Add(coord, i * timePerCell);
                }
            }

            return timeline;
        }

        private static ImpactTimeline BuildSquareTimeline(Vector2Int origin, int radius, IGridModel model)
        {
            var timeline = new ImpactTimeline();

            for (int r = 1; r <= radius; r++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    for (int dy = -r; dy <= r; dy++)
                    {
                        if (Mathf.Abs(dx) != r && Mathf.Abs(dy) != r) continue;

                        var coord = new Vector2Int(origin.x + dx, origin.y + dy);
                        
                        if (model.IsInRange(coord))
                            timeline.Add(coord, 0f);
                    }
                }
            }

            return timeline;
        }

        public static int[] BuildLineOffsets(int lineCount)
        {
            if (lineCount <= 0) return Array.Empty<int>();

            var offsets = new int[lineCount];
            offsets[0] = 0;

            for (int i = 1; i < lineCount; i++)
            {
                var distance = (i + 1) / 2;
                var sign = (i % 2 == 1) ? 1 : -1;
                offsets[i] = distance * sign;
            }

            return offsets;
        }
    }

    public class ImpactTimeline
    {
        private readonly List<ImpactEntry> _entries = new();
        public IReadOnlyList<ImpactEntry> Entries => _entries;
        public void Add(Vector2Int coord, float delay) => _entries.Add(new ImpactEntry(coord, delay));
        public void SortByDelay() => _entries.Sort((a, b) => a.Delay.CompareTo(b.Delay));
        
        public void SortByDelayThenCoord()
        {
            _entries.Sort((a, b) =>
            {
                var delayCompare = a.Delay.CompareTo(b.Delay);
                if (delayCompare != 0) return delayCompare;
        
                var xCompare = a.Coord.x.CompareTo(b.Coord.x);
                if (xCompare != 0) return xCompare;
     
                return a.Coord.y.CompareTo(b.Coord.y);
            });
        }
    }

    public readonly struct ImpactEntry
    {
        public readonly Vector2Int Coord;
        public readonly float Delay;

        public ImpactEntry(Vector2Int coord, float delay)
        {
            Coord = coord;
            Delay = delay;
        }
    }
}