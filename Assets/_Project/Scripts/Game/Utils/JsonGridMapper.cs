using System;
using Core.Item;
using Core.Level;

namespace Core.Utils
{
    public static class JsonGridMapper
    {
        private const int BoosterOffset = 100;
        private const int ObstacleOffset = 200;
        
        public static Enum Decode(int code)
        {
            if (code == 0)
            {
                return ItemType.None;
            }

            if (code < BoosterOffset)
            {
                return (ItemType)code;
            }

            if (code < ObstacleOffset)
            {
                return (BoosterType)(code - BoosterOffset);
            }

            return (ObstacleType)(code - ObstacleOffset);
        }
        
        public static int Encode(Enum value)
        {
            switch (value)
            {
                case ItemType itemType:
                    return (int)itemType;

                case BoosterType boosterType:
                    return BoosterOffset + (int)boosterType;

                case ObstacleType obstacleType:
                    return ObstacleOffset + (int)obstacleType;

                default:
                    return 0;
            }
        }
        
        public static int EncodeJson(JsonGridObjectType value)
        {
            return value switch
            {
                JsonGridObjectType.r  => Encode(ItemType.Red),
                JsonGridObjectType.g  => Encode(ItemType.Green),
                JsonGridObjectType.b  => Encode(ItemType.Blue),
                JsonGridObjectType.y  => Encode(ItemType.Yellow),

                JsonGridObjectType.bo => Encode(ObstacleType.Box),
                JsonGridObjectType.s  => Encode(ObstacleType.Stone),
                JsonGridObjectType.v  => Encode(ObstacleType.Vase),

                JsonGridObjectType.t  => Encode(BoosterType.Bomb),
                JsonGridObjectType.empty => Encode(ItemType.None),
                _ => 0
            };
        }
        
        public static JsonGridObjectType DecodeJson(int code)
        {
            if (code == 0)
            {
                return JsonGridObjectType.rand;
            }

            var decoded = Decode(code);

            return decoded switch
            {
                ItemType itemType => itemType switch
                {
                    ItemType.Red    => JsonGridObjectType.r,
                    ItemType.Green  => JsonGridObjectType.g,
                    ItemType.Blue   => JsonGridObjectType.b,
                    ItemType.Yellow => JsonGridObjectType.y,
                    _               => JsonGridObjectType.rand
                },

                BoosterType boosterType => boosterType switch
                {
                    BoosterType.Bomb => JsonGridObjectType.t,
                    _                => JsonGridObjectType.rand
                },

                ObstacleType obstacleType => obstacleType switch
                {
                    ObstacleType.Box   => JsonGridObjectType.bo,
                    ObstacleType.Stone => JsonGridObjectType.s,
                    ObstacleType.Vase  => JsonGridObjectType.v,
                    _                  => JsonGridObjectType.rand
                },

                _ => JsonGridObjectType.rand
            };
        }
    }
}
