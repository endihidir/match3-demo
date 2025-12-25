using System;

namespace Core.Level
{
    [Serializable]
    public class LevelJson
    {
        public int level_number;
        public int grid_width;
        public int grid_height;
        public int move_count;
        public string[] grid;
    }
}