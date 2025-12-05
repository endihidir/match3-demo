using System;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Core.Extensions
{
    public static class AsyncExtensions
    {
        public static UniTask AwaitAsync(this Action actionField, Func<Action, Action> reassign)
        {
            var tcs = new UniTaskCompletionSource();

            Action handler = null;

            handler = () =>
            {
                reassign(actionField - handler);
                tcs.TrySetResult();
            };

            reassign(actionField + handler);

            return tcs.Task;
        }
        
        public static UniTask AwaitClickAsync(this Button button)
        {
            var tcs = new UniTaskCompletionSource();
            UnityAction handler = null;

            handler = () =>
            {
                button.onClick.RemoveListener(handler);
                tcs.TrySetResult();
            };

            button.onClick.AddListener(handler);
            return tcs.Task;
        }

        public static UniTask AwaitAsync(this UnityEvent unityEvent)
        {
            var tcs = new UniTaskCompletionSource();
            UnityAction handler = null;

            handler = () =>
            {
                unityEvent.RemoveListener(handler);
                tcs.TrySetResult();
            };

            unityEvent.AddListener(handler);
            return tcs.Task;
        }

        public static UniTask<T> AwaitAsync<T>(this UnityEvent<T> unityEvent)
        {
            var tcs = new UniTaskCompletionSource<T>();
            UnityAction<T> handler = null;

            handler = v1 =>
            {
                unityEvent.RemoveListener(handler);
                tcs.TrySetResult(v1);
            };

            unityEvent.AddListener(handler);
            return tcs.Task;
        }
    }
}