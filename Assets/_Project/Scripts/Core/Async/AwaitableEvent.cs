using System;
using Cysharp.Threading.Tasks;

namespace Core.Async
{
    public class AwaitableEvent
    {
        private event Action Event;
        
        public void Invoke() => Event?.Invoke();
        public void AddListener(Action listener) => Event += listener;
        public void RemoveListener(Action listener) => Event -= listener;

        public UniTask WaitAsync()
        {
            var tcs = new UniTaskCompletionSource();
            Action handler = null;

            handler = () =>
            {
                Event -= handler;
                tcs.TrySetResult();
            };

            Event += handler;
            return tcs.Task;
        }

        public static AwaitableEvent operator +(AwaitableEvent ev, Action listener)
        {
            ev ??= new AwaitableEvent();
            ev.Event += listener;
            return ev;
        }

        public static AwaitableEvent operator -(AwaitableEvent ev, Action listener)
        {
            if (ev == null) return null;
            ev.Event -= listener;
            return ev;
        }
    }

    public class AwaitableEvent<T>
    {
        private event Action<T> Event;
        
        public void Invoke(T v1) => Event?.Invoke(v1);
        public void AddListener(Action<T> listener) => Event += listener;
        public void RemoveListener(Action<T> listener) => Event -= listener;

        public UniTask<T> WaitAsync()
        {
            var tcs = new UniTaskCompletionSource<T>();
            Action<T> handler = null;

            handler = v1 =>
            {
                Event -= handler;
                tcs.TrySetResult(v1);
            };

            Event += handler;
            return tcs.Task;
        }

        public static AwaitableEvent<T> operator +(AwaitableEvent<T> ev, Action<T> listener)
        {
            ev ??= new AwaitableEvent<T>();
            ev.Event += listener;
            return ev;
        }

        public static AwaitableEvent<T> operator -(AwaitableEvent<T> ev, Action<T> listener)
        {
            if (ev == null) return null;
            ev.Event -= listener;
            return ev;
        }
    }

    public class AwaitableEvent<T1, T2>
    {
        private event Action<T1, T2> Event;
        
        public void Invoke(T1 v1, T2 v2) => Event?.Invoke(v1, v2);
        public void AddListener(Action<T1, T2> listener) => Event += listener;
        public void RemoveListener(Action<T1, T2> listener) => Event -= listener;

        public UniTask<(T1, T2)> WaitAsync()
        {
            var tcs = new UniTaskCompletionSource<(T1, T2)>();
            Action<T1, T2> handler = null;

            handler = (v1, v2) =>
            {
                Event -= handler;
                tcs.TrySetResult((v1, v2));
            };

            Event += handler;
            return tcs.Task;
        }

        public static AwaitableEvent<T1, T2> operator +(AwaitableEvent<T1, T2> ev, Action<T1, T2> listener)
        {
            ev ??= new AwaitableEvent<T1, T2>();
            ev.Event += listener;
            return ev;
        }

        public static AwaitableEvent<T1, T2> operator -(AwaitableEvent<T1, T2> ev, Action<T1, T2> listener)
        {
            if (ev == null) return null;
            ev.Event -= listener;
            return ev;
        }
    }

    public class AwaitableEvent<T1, T2, T3>
    {
        private event Action<T1, T2, T3> Event;
        
        public void Invoke(T1 v1, T2 v2, T3 v3) => Event?.Invoke(v1, v2, v3);
        public void AddListener(Action<T1, T2, T3> listener) => Event += listener;
        public void RemoveListener(Action<T1, T2, T3> listener) => Event -= listener;

        public UniTask<(T1, T2, T3)> WaitAsync()
        {
            var tcs = new UniTaskCompletionSource<(T1, T2, T3)>();
            Action<T1, T2, T3> handler = null;

            handler = (v1, v2, v3) =>
            {
                Event -= handler;
                tcs.TrySetResult((v1, v2, v3));
            };

            Event += handler;
            return tcs.Task;
        }

        public static AwaitableEvent<T1, T2, T3> operator +(AwaitableEvent<T1, T2, T3> ev, Action<T1, T2, T3> listener)
        {
            ev ??= new AwaitableEvent<T1, T2, T3>();
            ev.Event += listener;
            return ev;
        }

        public static AwaitableEvent<T1, T2, T3> operator -(AwaitableEvent<T1, T2, T3> ev, Action<T1, T2, T3> listener)
        {
            if (ev == null) return null;
            ev.Event -= listener;
            return ev;
        }
    }
}