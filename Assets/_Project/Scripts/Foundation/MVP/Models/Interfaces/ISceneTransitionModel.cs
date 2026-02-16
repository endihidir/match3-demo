namespace Core.Models
{
    public interface ISceneTransitionModel
    {
        float FillAmount { get; }
        float TargetRatio { get; }
        void SetTargetRatio(float val);
        void UpdateData();
        void ResetProgress();
    }
}