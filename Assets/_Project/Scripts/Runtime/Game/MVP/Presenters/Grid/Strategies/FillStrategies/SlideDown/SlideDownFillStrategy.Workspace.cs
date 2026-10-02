using System;

namespace Game.Grid.Strategies
{
    public sealed partial class SlideDownFillStrategy
    {
        private int[] _spawnStackByX = Array.Empty<int>();

        private void PrepareForRun(int width)
        {
            if (_spawnStackByX.Length < width)
                Array.Resize(ref _spawnStackByX, Math.Max(width, _spawnStackByX.Length * 2));

            Array.Clear(_spawnStackByX, 0, width);
        }
    }
}