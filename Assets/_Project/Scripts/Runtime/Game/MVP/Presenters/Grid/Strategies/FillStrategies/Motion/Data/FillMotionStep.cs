using UnityEngine;

namespace Game.Grid.Strategies.Data
{
    public readonly struct FillMotionStep
    {
        public readonly Vector2Int From;
        public readonly Vector2Int To;
        public readonly float StartTime;
        public readonly float EndTime;
        public readonly float ReleaseTime;
        public readonly float StartSpeed;
        public readonly float EndSpeed;
        public readonly float Length;

        public FillMotionStep(Vector2Int from, Vector2Int to, float startTime, float endTime, float releaseTime, float startSpeed, float endSpeed, float length)
        {
            From = from;
            To = to;
            StartTime = startTime;
            EndTime = endTime;
            ReleaseTime = releaseTime;
            StartSpeed = startSpeed;
            EndSpeed = endSpeed;
            Length = length;
        }

        public FillMotionStep WithReleaseTime(float releaseTime)
        {
            return new FillMotionStep(From, To, StartTime, EndTime, releaseTime, StartSpeed, EndSpeed, Length);
        }
    }
}