using Game.Grid.Item;

namespace Game.Extensions
{
    public static class BoosterExtensions
    {
        public static BoosterFamily ToFamily(this BoosterType type)
        {
            return type switch
            {
                BoosterType.RocketHorizontal => BoosterFamily.Rocket,
                BoosterType.RocketVertical => BoosterFamily.Rocket,
                BoosterType.Bomb => BoosterFamily.Bomb,
                BoosterType.Fly => BoosterFamily.Fly,
                BoosterType.Orb => BoosterFamily.Orb,
                _ => BoosterFamily.Rocket
            };
        }
    }
}