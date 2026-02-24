using System;
using Game.Level.Data;

namespace Core.Utils
{
    public static class LevelJsonEditorUtils
    {
        public static JsonGridObjectType[,] ToEditorGrid(LevelJson levelJson)
        {
            var width = levelJson.grid_width;
            var height = levelJson.grid_height;

            var result = new JsonGridObjectType[height, width];

            if (levelJson.grid == null || levelJson.grid.Length != width * height)
            {
                EditorLogger.LogError("LevelJson.grid size does not match grid dimensions.");
               
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        result[y, x] = JsonGridObjectType.rand;
                    }
                }

                return result;
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var index = y * width + x;
                    var cellString = levelJson.grid[index];

                    if (!Enum.TryParse(cellString, out JsonGridObjectType parsed))
                    {
                        parsed = JsonGridObjectType.rand;
                    }

                    result[y, x] = parsed;
                }
            }

            return result;
        }
        
        
        public static LevelJson ConvertToLevelJson(int levelNumber, int gridWidth, int gridHeight, int moveCount, JsonGridObjectType[,] items)
        {
            var levelJson = new LevelJson
            {
                level_number = levelNumber,
                grid_width = gridWidth,
                grid_height = gridHeight,
                move_count = moveCount,
                grid = new string[gridWidth * gridHeight]
            };

            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    var index = y * gridWidth + x;
                    levelJson.grid[index] = items[y, x].ToString();
                }
            }

            return levelJson;
        }

        public static string BuildPrettyPrintedJson(int levelNumber, int gridWidth, int gridHeight, int moveCount, JsonGridObjectType[,] items)
        {
            var sb = new System.Text.StringBuilder(); 
            
            sb.AppendLine("{"); 
            sb.AppendLine($" \"level_number\": {levelNumber},"); 
            sb.AppendLine($" \"grid_width\": {gridWidth},"); 
            sb.AppendLine($" \"grid_height\": {gridHeight},"); 
            sb.AppendLine($" \"move_count\": {moveCount},"); 
            sb.Append(" \"grid\": ["); 
            
            var total = gridWidth * gridHeight; 
            var index = 0;

            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    sb.Append($"\"{items[y, x]}\""); 
                    
                    if (index < total - 1) 
                        sb.Append(", "); index++;
                }
            } 
            
            sb.AppendLine("]"); 
            sb.Append("}"); 
            return sb.ToString();
        }
    }
}