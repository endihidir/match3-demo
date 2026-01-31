namespace Core.Models
{
    public interface ILevelProgressionWriter
    {
        void SetLevel(int levelIndex);
        void AdvanceLevel();
        void ResetProgress();
    }
}