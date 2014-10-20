// Copyright (c) 2012 Daniel Grunwald
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading.Tasks;

// This is a minimal implementation of the stuff needed by the C# 5 Beta
// compiler for 'async'/'await' feature.
// Simply add this file to a .NET 4.0 project, and 'async'/'await' will work
// in your code without requiring your users to install .NET 4.5.
// However, you still need the C# 5 compiler for compiling your code.
// Caveats:
//  - Stack trace is lost when rethrowing exceptions
//  - Unhandled Exceptions in 'async void' methods are re-thrown immediately
//    and not posted to the synchronization context
//  - Flowing ExecutionContext is not supported
//  --> do not use this in security critical code (APTCA)
//  - The code is not optimized. Tasks are always created, repeated boxing may occur.

namespace System.Threading.Tasks
{
    internal static class TaskEx
    {
        public static TaskAwaiter GetAwaiter(this Task task)
        {
            return new TaskAwaiter(task);
        }

        public static TaskAwaiter<T> GetAwaiter<T>(this Task<T> task)
        {
            return new TaskAwaiter<T>(task);
        }
    }

    internal struct TaskAwaiter : INotifyCompletion
    {
        readonly Task task;

        internal TaskAwaiter(Task task)
        {
            this.task = task;
        }

        internal static TaskScheduler TaskScheduler
        {
            get
            {
                if (SynchronizationContext.Current == null)
                    return TaskScheduler.Default;
                else
                    return TaskScheduler.FromCurrentSynchronizationContext();
            }
        }

        public bool IsCompleted
        {
            get { return task.IsCompleted; }
        }

        public void OnCompleted(Action continuation)
        {
            this.task.ContinueWith(
                delegate(Task task)
                {
                    continuation();
                }, TaskAwaiter.TaskScheduler);
        }

        public void GetResult()
        {
            try
            {
                task.Wait();
            }
            catch (AggregateException ex)
            {
                throw ex.InnerExceptions[0];
            }
        }
    }

    internal struct TaskAwaiter<T> : INotifyCompletion
    {
        readonly Task<T> task;

        internal TaskAwaiter(Task<T> task)
        {
            this.task = task;
        }

        public bool IsCompleted
        {
            get { return task.IsCompleted; }
        }

        public void OnCompleted(Action continuation)
        {
            this.task.ContinueWith(
                delegate(Task<T> task)
                {
                    continuation();
                }, TaskAwaiter.TaskScheduler);
        }

        public T GetResult()
        {
            try
            {
                return task.Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerExceptions[0];
            }
        }
    }
}

namespace System.Runtime.CompilerServices
{
    internal interface INotifyCompletion
    {
        void OnCompleted(Action continuation);
    }

    internal interface ICriticalNotifyCompletion : INotifyCompletion
    {
        [SecurityCritical]
        void UnsafeOnCompleted(Action continuation);
    }

    internal interface IAsyncStateMachine
    {
        void MoveNext();
        void SetStateMachine(IAsyncStateMachine stateMachine);
    }

    internal struct AsyncVoidMethodBuilder
    {
        public static AsyncVoidMethodBuilder Create()
        {
            return new AsyncVoidMethodBuilder();
        }

        public void SetException(Exception exception)
        {
            throw exception;
        }

        public void SetResult()
        {
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            // Should not get called as we don't implement the optimization that this method is used for.
            throw new NotImplementedException();
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }
    }

    internal struct AsyncTaskMethodBuilder
    {
        TaskCompletionSource<object> tcs;

        public Task Task { get { return tcs.Task; } }

        public static AsyncTaskMethodBuilder Create()
        {
            AsyncTaskMethodBuilder b;
            b.tcs = new TaskCompletionSource<object>();
            return b;
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            // Should not get called as we don't implement the optimization that this method is used for.
            throw new NotImplementedException();
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        public void SetResult()
        {
            tcs.SetResult(null);
        }

        public void SetException(Exception exception)
        {
            tcs.SetException(exception);
        }
    }

    internal struct AsyncTaskMethodBuilder<T>
    {
        TaskCompletionSource<T> tcs;

        public Task<T> Task { get { return tcs.Task; } }

        public static AsyncTaskMethodBuilder<T> Create()
        {
            AsyncTaskMethodBuilder<T> b;
            b.tcs = new TaskCompletionSource<T>();
            return b;
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            // Should not get called as we don't implement the optimization that this method is used for.
            throw new NotImplementedException();
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            AwaitOnCompleted(ref awaiter, ref stateMachine);
        }

        public void SetResult(T result)
        {
            tcs.SetResult(result);
        }

        public void SetException(Exception exception)
        {
            tcs.SetException(exception);
        }
    }
}
