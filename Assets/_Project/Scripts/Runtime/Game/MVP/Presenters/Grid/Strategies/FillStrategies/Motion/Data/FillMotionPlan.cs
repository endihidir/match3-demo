using System.Collections.Generic;
using Game.Grid.Item;

namespace Game.Grid.Strategies.Data
{
    public sealed class FillMotionPlan
    {
        public readonly List<FillMotionStep> Steps = new(16);

        public BaseGridObject Item { get; private set; }
        public int Version { get; set; }
        public int NewStepStart { get; set; }
        public float ReadyTime { get; set; }
        public float ReadySpeed { get; set; }
        public bool IsTouched { get; set; }

        public float EndTime => Steps.Count > 0 ? Steps[^1].EndTime : float.NegativeInfinity;

        public void Reset(BaseGridObject item, float readyTime)
        {
            Steps.Clear();
            Item = item;
            Version = 0;
            NewStepStart = 0;
            ReadyTime = readyTime;
            ReadySpeed = 0f;
        }
    }
}