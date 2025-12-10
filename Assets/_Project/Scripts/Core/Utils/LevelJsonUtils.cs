using System;
using System.Collections.Generic;
using Core.Item;
using Core.Level;
using UnityEngine;

namespace Core.Utils
{
    public static class LevelJsonUtils
    {
        // ========= RUNTIME =========
        private struct ObstacleCounts
        {
            public int Boxes;
            public int Stones;
            public int Vases;
        }
        
        public static LevelDefinition ParseToLevelDefinition(TextAsset jsonFile)
        {
            var levelJson = JsonUtility.FromJson<LevelJson>(jsonFile.text);
            var levelContent = ProcessLevelJson(levelJson);
            return new LevelDefinition(levelJson.level_number, levelJson.move_count, levelContent.gridObjectTypes, levelContent.levelGoals);
        }
        
        public static LevelContentData ProcessLevelJson(LevelJson levelJson)
        {
            var boardData = CreateEmptyBoard(levelJson);
            var obstacleCounts = FillGridAndCountObstacles(levelJson, boardData.gridObjectTypes);
            boardData.levelGoals = CreateLevelGoals(obstacleCounts);
            return boardData;
        }

        private static LevelContentData CreateEmptyBoard(LevelJson levelJson)
        {
            return new LevelContentData
            {
                gridObjectTypes = new GridObjectTypeData[levelJson.grid_width, levelJson.grid_height],
                levelGoals = new List<LevelGoal>()
            };
        }

        private static ObstacleCounts FillGridAndCountObstacles(LevelJson levelJson, GridObjectTypeData[,] grid)
        {
            var counts = new ObstacleCounts();

            var height = levelJson.grid_height;
            var width = levelJson.grid_width;
            var expectedLength = width * height;

            if (levelJson.grid == null || levelJson.grid.Length != expectedLength)
            {
                EditorLogger.Log($"Grid length mismatch. Expected: {expectedLength}, Actual: {levelJson.grid?.Length ?? 0}");
            }

            for (var index = 0; index < expectedLength; index++)
            {
                var coordinate = CoordinateUtils.ToCoordinate(index, width);
                var col = coordinate.x;
                var row = coordinate.y;

                GridObjectTypeData cellData;

                if (levelJson.grid != null && index < levelJson.grid.Length)
                {
                    var typeStr = levelJson.grid[index];
                    cellData = DecodeCell(typeStr, ref counts);
                }
                else
                {
                    cellData = CreateRandomItemCell();
                }

                grid[col, row] = cellData;
            }

            return counts;
        }

        private static GridObjectTypeData DecodeCell(string typeStr, ref ObstacleCounts counts)
        {
            if (!Enum.TryParse(typeStr, out JsonGridObjectType jsonType))
            {
                EditorLogger.Log("Invalid grid object type: " + typeStr);
                return CreateRandomItemCell();
            }

            var encoded = JsonGridMapper.EncodeJson(jsonType);

            if (encoded == 0 && jsonType == JsonGridObjectType.rand)
            {
                var randomItem = GetRandomEnumValue<ItemType>();
                return CreateFromEnum(randomItem, ref counts);
            }

            var decodedEnum = JsonGridMapper.Decode(encoded);
            return CreateFromEnum(decodedEnum, ref counts);
        }

        private static GridObjectTypeData CreateFromEnum(Enum decodedEnum, ref ObstacleCounts counts)
        {
            switch (decodedEnum)
            {
                case ItemType itemType:
                    return new GridObjectTypeData
                    {
                        gridItemKind = GridItemKind.Regular,
                        typeId = (int)itemType
                    };

                case BoosterType boosterType:
                    return new GridObjectTypeData
                    {
                        gridItemKind = GridItemKind.Booster,
                        typeId = (int)boosterType
                    };

                case ObstacleType obstacleType:
                    IncrementObstacleCount(obstacleType, ref counts);
                    return new GridObjectTypeData
                    {
                        gridItemKind = GridItemKind.Obstacle,
                        typeId = (int)obstacleType
                    };

                default:
                    return CreateRandomItemCell();
            }
        }

        private static void IncrementObstacleCount(ObstacleType obstacleType, ref ObstacleCounts counts)
        {
            switch (obstacleType)
            {
                case ObstacleType.Box:
                    counts.Boxes++;
                    break;
                case ObstacleType.Stone:
                    counts.Stones++;
                    break;
                case ObstacleType.Vase:
                    counts.Vases++;
                    break;
            }
        }

        private static GridObjectTypeData CreateRandomItemCell()
        {
            var randomItem = GetRandomEnumValue<ItemType>();

            return new GridObjectTypeData
            {
                gridItemKind = GridItemKind.Regular,
                typeId = (int)randomItem
            };
        }

        private static T GetRandomEnumValue<T>() where T : Enum
        {
            var values = (T[])Enum.GetValues(typeof(T));

            var valid = new List<T>();
            foreach (var v in values)
            {
                if (Convert.ToInt32(v) != 0)
                    valid.Add(v);
            }

            var index = UnityEngine.Random.Range(0, valid.Count);
            return valid[index];
        }

        private static List<LevelGoal> CreateLevelGoals(ObstacleCounts counts)
        {
            var goals = new List<LevelGoal>();

            if (counts.Boxes > 0)
            {
                goals.Add(new LevelGoal
                {
                    ObstacleType = ObstacleType.Box,
                    Count = counts.Boxes
                });
            }

            if (counts.Stones > 0)
            {
                goals.Add(new LevelGoal
                {
                    ObstacleType = ObstacleType.Stone,
                    Count = counts.Stones
                });
            }

            if (counts.Vases > 0)
            {
                goals.Add(new LevelGoal
                {
                    ObstacleType = ObstacleType.Vase,
                    Count = counts.Vases
                });
            }

            return goals;
        }

        // ========= EDITOR =========
        
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
