using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace Game.Grid.Services
{
    public sealed class GridInputService : IGridInputService, ITickable, IDisposable
    {
        private readonly InputActions _actions = new();
        public event Action<Vector2, Vector2Int> OnInputGet;

        private Vector2 _startPos;
        private Vector2 _lastPos;

        private bool _isPointerOverUI;

        public GridInputService()
        {
            Input.multiTouchEnabled = false;
        }

        public void Enable()
        {
            _actions.Enable();
            _actions.Board.Press.started += OnPressStarted;
            _actions.Board.Press.canceled += OnPressCanceled;
            _actions.Board.Position.performed += OnPositionPerformed;
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

            var threshold = GetSwipeThreshold();

            var dir = GetSwipeDirection(_startPos, endPos, threshold);

            OnInputGet?.Invoke(_startPos, dir);
        }

        private static Vector2Int GetSwipeDirection(Vector2 start, Vector2 end, float threshold)
        {
            var delta = end - start;

            if (delta.sqrMagnitude < threshold * threshold)
                return Vector2Int.zero;

            var ax = Mathf.Abs(delta.x);
            var ay = Mathf.Abs(delta.y);

            if (ax > ay)
                return delta.x > 0f ? Vector2Int.right : Vector2Int.left;

            return delta.y > 0f ? Vector2Int.up : Vector2Int.down;
        }

        public void Disable()
        {
            _actions.Board.Press.started -= OnPressStarted;
            _actions.Board.Press.canceled -= OnPressCanceled;
            _actions.Board.Position.performed -= OnPositionPerformed;
            _actions.Disable();
        }

        public void Dispose()
        {
            Disable();
        }

        private static float GetSwipeThreshold()
        {
            var minScreenSide = Mathf.Min(Screen.width, Screen.height);
            return minScreenSide * 0.02f;
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