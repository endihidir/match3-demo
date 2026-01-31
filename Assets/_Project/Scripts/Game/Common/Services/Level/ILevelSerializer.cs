namespace Core.Level
{
    public interface ILevelSerializer
    {
        LevelDefinition SerializeToLevelDefinition(int level);
    }
}