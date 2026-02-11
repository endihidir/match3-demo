using System;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Modules
{
    public class AnimatorModule : MonoBehaviour
    {
        [field: SerializeField, Required] public Animator Animator { get; private set; }
        
        public event Action OnComplete;
        
        private int _currentLayer = 0;
        private int _targetStateHash;
        
        public bool IsAlive
        {
            get
            {
                if (!Animator) return false;
            
                var stateInfo = Animator.GetCurrentAnimatorStateInfo(_currentLayer);
            
                if (stateInfo.shortNameHash != _targetStateHash) return false;
            
                return stateInfo.normalizedTime < 1f;
            }
        }
    
        public void Play()
        {
            if (!Animator) return;
            
            _targetStateHash = Animator.GetCurrentAnimatorStateInfo(_currentLayer).shortNameHash;
            Animator.enabled = true;
        }
    
        public void Play(string stateName)
        {
            if (!Animator) return;
            
            _targetStateHash = Animator.StringToHash(stateName);
            Animator.enabled = true;
            Animator.Play(stateName);
        }
    
        public void Play(int stateHash)
        {
            if (!Animator) return;
            
            _targetStateHash = stateHash;
            Animator.enabled = true;
            Animator.Play(stateHash);
        }

        public void SetCurrentLayer(int layer)
        {
            if (!Animator) return;
            _currentLayer = layer;
        }
    
        public void SetTrigger(string triggerName)
        {
            if (!Animator) return;
            Animator.SetTrigger(triggerName);
        }
    
        public void SetTrigger(int triggerHash)
        {
            if (!Animator) return;
            Animator.SetTrigger(triggerHash);
        }
    
        public void SetBool(string paramName, bool value)
        {
            if (!Animator) return;
            Animator.SetBool(paramName, value);
        }
    
        public void SetFloat(string paramName, float value)
        {
            if (!Animator) return;
            Animator.SetFloat(paramName, value);
        }
    
        public void SetSpeed(float speed)
        {
            if (!Animator) return;
            Animator.speed = speed;
        }
    
        public void Pause() => SetSpeed(0f);
    
        public void Resume() => SetSpeed(1f);
    
        public float GetCurrentNormalizedTime(int layer = 0)
        {
            if (!Animator) return 0f;
            return Animator.GetCurrentAnimatorStateInfo(layer).normalizedTime;
        }
    
        public bool IsInState(string stateName, int layer = 0)
        {
            if (!Animator) return false;
            return Animator.GetCurrentAnimatorStateInfo(layer).IsName(stateName);
        }
        
        public void InvokeOnComplete() => OnComplete?.Invoke();
        
        public void Stop()
        {
            if (!Animator) return;
            _targetStateHash = 0;
            Animator.enabled = false;
        }
    
        public void Rebind()
        {
            if (!Animator) return;
    
            _targetStateHash = 0;
            Animator.Rebind();
        }
    }
}