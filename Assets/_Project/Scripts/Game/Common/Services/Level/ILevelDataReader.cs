namespace Core.Level
{
    public interface ILevelDataReader
    {
        int LevelSize { get; }
        LevelDefinition GetLevelDefinition(int index);
    }
}