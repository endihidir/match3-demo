using System;
using Core.Item;
using Core.Level;

namespace Core.Utils
{
    public static class JsonGridMapper
    {
        private const int BoosterOffset = 100;
        private const int ObstacleOffset = 200;
        
        public static Enum Decode(int code) => code switch
        {
            0 => ItemType.None,
            < BoosterOffset => (ItemType)code,
            < ObstacleOffset => (BoosterType)(code - BoosterOffset),
            _ => (ObstacleType)(code - ObstacleOffset)
        };
        
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
                    BoosterType.Bomb             => JsonGridObjectType.t,
                    BoosterType.RocketHorizontal => JsonGridObjectType.ro_h,
                    BoosterType.RocketVertical   => JsonGridObjectType.ro_v,
                    _                            => JsonGridObjectType.rand
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

        public static int Encode(Enum value) => value switch
        {
            ItemType itemType => (int)itemType,
            BoosterType boosterType => BoosterOffset + (int)boosterType,
            ObstacleType obstacleType => ObstacleOffset + (int)obstacleType,
            _ => 0
        };

        public static int EncodeJson(JsonGridObjectType value) => value switch
        {
            JsonGridObjectType.r  => Encode(ItemType.Red),
            JsonGridObjectType.g  => Encode(ItemType.Green),
            JsonGridObjectType.b  => Encode(ItemType.Blue),
            JsonGridObjectType.y  => Encode(ItemType.Yellow),

            JsonGridObjectType.bo => Encode(ObstacleType.Box),
            JsonGridObjectType.s  => Encode(ObstacleType.Stone),
            JsonGridObjectType.v  => Encode(ObstacleType.Vase),

            JsonGridObjectType.t     => Encode(BoosterType.Bomb),
            JsonGridObjectType.ro_h  => Encode(BoosterType.RocketHorizontal),
            JsonGridObjectType.ro_v  => Encode(BoosterType.RocketVertical),
            JsonGridObjectType.empty => Encode(ItemType.None),
            _ => 0
        };
    }
}
