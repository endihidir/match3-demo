using Core.SceneService;
using UnityEngine;

namespace Core.Models
{
    public class SceneTransitionModel : ISceneTransitionModel
    {
        private readonly float _progressSpeed;
        public float FillAmount { get; private set; }
        public float TargetRatio { get; private set; }

        public SceneTransitionModel(ISceneLoadState sceneLoadState) => _progressSpeed = sceneLoadState.ProgressSpeed;

        public void SetTargetRatio(float val)
        {
            val = Mathf.Clamp01(val);
            TargetRatio = Mathf.Max(TargetRatio, val);
        }

        public void UpdateData()
        {
            var diff = Mathf.Abs(TargetRatio - FillAmount);
            
            if (diff <= 0.001f)
            {
                FillAmount = TargetRatio;
                return;
            }

            var t = Mathf.Clamp01(Time.deltaTime * _progressSpeed * diff);
            FillAmount = Mathf.MoveTowards(FillAmount, TargetRatio, t);
            FillAmount = Mathf.Clamp01(FillAmount);
        }

        public void ResetProgress() 
        {
            TargetRatio = 0f;
            FillAmount = 0f;    
        }
    }
}