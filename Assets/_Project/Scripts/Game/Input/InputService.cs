using System;
using Core.Models;
using Core.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace Core.Services
{
    public interface IInputService
    {
        event Action<Vector2, Direction2D> OnInput;
        void Enable();
        void Disable();
    }

    public class InputService : IInputService, ITickable, IDisposable
    {
        private const float SWIPE_THRESHOLD = 30f;
        private readonly InputActions _actions = new();
        public event Action<Vector2, Direction2D> OnInput;

        private Vector2 _startPos;
        private Vector2 _lastPos;

        private bool _isPointerOverUI;

        public InputService()
        {
            Input.multiTouchEnabled = false;
        }

        public void Enable()
        {
            _actions.Enable();
            _actions.Board.Press.started += OnPressStarted;
            _actions.Board.Press.canceled += OnPressCanceled;
            _actions.Board.Position.performed += OnPositionPerformed;
            EditorLogger.Log("Input Service Enabled");
        }

        private void OnPositionPerformed(InputAction.CallbackContext ctx)
        {
            _lastPos = ctx.ReadValue<Vector2>();
        }

        private void OnPressStarted(InputAction.CallbackContext ctx)
        {
            _startPos = _actions.Board.Position.ReadValue<Vector2>();
            _lastPos = _startPos;
        }

        private void OnPressCanceled(InputAction.CallbackContext ctx)
        {
            if(_isPointerOverUI) return;
            
            var endPos = _actions.Board.Position.ReadValue<Vector2>();
            
            var dir = GetSwipeDirection(_startPos, endPos, SWIPE_THRESHOLD);
            
            OnInput?.Invoke(_startPos, dir);
        }

        private static Direction2D GetSwipeDirection(Vector2 start, Vector2 end, float threshold)
        {
            var delta = end - start;

            if (delta.sqrMagnitude < threshold * threshold)
                return Direction2D.Self;

            var ax = Mathf.Abs(delta.x);
            var ay = Mathf.Abs(delta.y);

            if (ax > ay)
                return delta.x > 0f ? Direction2D.Right : Direction2D.Left;

            return delta.y > 0f ? Direction2D.Up : Direction2D.Down;
        }

        public void Disable()
        {
            _actions.Board.Press.started -= OnPressStarted;
            _actions.Board.Press.canceled -= OnPressCanceled;
            _actions.Board.Position.performed -= OnPositionPerformed;
            _actions.Disable();
            EditorLogger.Log("Input Service Disabled");
        }

        public void Dispose()
        {
            Disable();
        }
        
        private static bool GetPointerOverUI()
        {
            if (!EventSystem.current) return false;
            if (EventSystem.current.IsPointerOverGameObject()) return true;

            if (Touchscreen.current == null) return false;
            var touch = Touchscreen.current.primaryTouch;
            if (!touch.press.isPressed) return false;
            
            var touchId = touch.touchId.ReadValue();
            return EventSystem.current.IsPointerOverGameObject(touchId);
        }

        public void Tick() => _isPointerOverUI = GetPointerOverUI();
    }
}
