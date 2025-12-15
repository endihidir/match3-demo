using System;
using System.Collections.Generic;
using Core.Item;
using Core.Level;
using UnityEngine;
using Random = System.Random;

namespace Core.Utils
{
    public static class LevelJsonRuntimeUtils
    {
        private struct ObstacleCounts
        {
            public int Boxes;
            public int Stones;
            public int Vases;
        }
        
        public static LevelDefinition ParseToLevelDefinition(TextAsset jsonFile, bool preventInitialMatches = true, bool useSeededPattern = false, int seedOverride = 0)
        {
            var levelJson = JsonUtility.FromJson<LevelJson>(jsonFile.text);
            var levelContent = ProcessLevelJson(levelJson, preventInitialMatches, useSeededPattern, seedOverride);
            return new LevelDefinition(levelJson.level_number, levelJson.move_count, levelContent.gridObjectTypes, levelContent.levelGoals);
        }
        
        public static LevelContentData ProcessLevelJson(LevelJson levelJson, bool preventInitialMatches = true, bool useSeededPattern = false, int seedOverride = 0)
        {
            var rng = LevelGridRandomUtil.CreateRng(levelJson.level_number, useSeededPattern, seedOverride);

            var boardData = CreateEmptyBoard(levelJson);
            var randomMask = new bool[levelJson.grid_width, levelJson.grid_height];

            var obstacleCounts = FillGridAndCountObstacles(levelJson, boardData.gridObjectTypes, randomMask, rng);

            if (preventInitialMatches)
            {
                InitialMatchCleanupUtil.RemoveInitialMatches(boardData.gridObjectTypes, randomMask, levelJson.grid_width, levelJson.grid_height, rng);
            }

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

        private static ObstacleCounts FillGridAndCountObstacles(LevelJson levelJson, GridObjectTypeData[,] grid, bool[,] randomMask, Random rng)
        {
            var counts = new ObstacleCounts();

            var height = levelJson.grid_height;
            var width = levelJson.grid_width;
            var expectedLength = width * height;

            for (var index = 0; index < expectedLength; index++)
            {
                var coordinate = CoordinateUtils.ToCoordinate(index, width);
                var col = coordinate.x;
                var row = coordinate.y;

                GridObjectTypeData cellData;
                var isRandomOrigin = false;

                if (levelJson.grid != null && index < levelJson.grid.Length)
                {
                    var typeStr = levelJson.grid[index];

                    if (Enum.TryParse(typeStr, out JsonGridObjectType jt) && jt == JsonGridObjectType.rand)
                        isRandomOrigin = true;

                    cellData = DecodeCell(typeStr, ref counts, rng);
                }
                else
                {
                    isRandomOrigin = true;
                    cellData = CreateRandomItemCell(rng);
                }

                grid[col, row] = cellData;
                randomMask[col, row] = isRandomOrigin;
            }

            return counts;
        }

        private static GridObjectTypeData DecodeCell(string typeStr, ref ObstacleCounts counts, Random rng)
        {
            if (!Enum.TryParse(typeStr, out JsonGridObjectType jsonType))
            {
                EditorLogger.Log("Invalid grid object type: " + typeStr);
                return CreateRandomItemCell(rng);
            }

            var encoded = JsonGridMapper.EncodeJson(jsonType);

            if (encoded == 0 && jsonType == JsonGridObjectType.rand)
            {
                var id = LevelGridRandomUtil.GetRandomItemTypeId(rng);
                return new GridObjectTypeData(GridItemKind.Regular, id);
            }

            var decodedEnum = JsonGridMapper.Decode(encoded);
            return CreateFromEnum(decodedEnum, ref counts, rng);
        }

        private static GridObjectTypeData CreateFromEnum(Enum decodedEnum, ref ObstacleCounts counts, Random rng)
        {
            switch (decodedEnum)
            {
                case ItemType itemType:
                    return new GridObjectTypeData(GridItemKind.Regular, (int)itemType);

                case BoosterType boosterType:
                    return new GridObjectTypeData(GridItemKind.Booster, (int)boosterType);

                case ObstacleType obstacleType:
                    IncrementObstacleCount(obstacleType, ref counts);
                    return new GridObjectTypeData(GridItemKind.Obstacle, (int)obstacleType);

                case GridItemKind.None:
                    return new GridObjectTypeData(GridItemKind.None, -1);

                default:
                    return CreateRandomItemCell(rng);
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

        private static GridObjectTypeData CreateRandomItemCell(Random rng)
        {
            var id = LevelGridRandomUtil.GetRandomItemTypeId(rng);
            return new GridObjectTypeData(GridItemKind.Regular, id);
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
    }
}