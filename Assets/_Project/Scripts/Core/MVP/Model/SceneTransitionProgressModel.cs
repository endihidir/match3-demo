using Core.MVPContext.Interfaces;
using Core.SceneService;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Models
{
    public interface ISceneTransitionProgressModel : IModel
    {
        float FillAmount { get; }
        float TargetRatio { get; }
        void SetTargetRatio(float val);
        void UpdateData();
        void ResetProgress();
    }
    
    public class SceneTransitionProgressModel : ISceneTransitionProgressModel
    {
        private readonly float _progressSpeed;
        public float FillAmount { get; private set; }
        public float TargetRatio { get; private set; }

        private bool _isRefreshed = false;

        public SceneTransitionProgressModel(ISceneLoadInfo sceneLoadInfo) => _progressSpeed = sceneLoadInfo.ProgressSpeed;

        public void SetTargetRatio(float val)
        {
            val = Mathf.Clamp01(val);
            TargetRatio = Mathf.Max(TargetRatio, val);
        }

        public void UpdateData()
        {
            if (_isRefreshed) return;
            
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
            Refresh().Forget();
        }

        private async UniTask Refresh()
        {
            _isRefreshed = true;
            await UniTask.WaitForSeconds(0.15f);
            _isRefreshed = false;
        }
    }
}