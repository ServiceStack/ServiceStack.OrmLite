using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStack.OrmLite
{
    public static class AsyncUtils
    {
        public static Task<T> FromResult<T>(T result)
        {
            var taskSource = new TaskCompletionSource<T>();
            taskSource.SetResult(result);
            return taskSource.Task;
        }

        public static Task<To> Cast<From, To>(this Task<From> task) where To : From
        {
            return task.Then(x => (To) x);
        }

        public static Task<To> Then<From, To>(this Task<From> task, Func<From, To> fn)
        {
            var tcs = new TaskCompletionSource<To>();
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    tcs.TrySetException(t.Exception.InnerExceptions);
                else if (t.IsCanceled)
                    tcs.TrySetCanceled();
                else
                    tcs.TrySetResult(fn(t.Result));
            }, TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }
    }
}