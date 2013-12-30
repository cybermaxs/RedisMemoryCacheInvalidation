using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace RedisMemoryCacheInvalidation
{
    [ExcludeFromCodeCoverage]
    public static class TaskHelper
    {
        private static class TaskRunners<T, TResult>
        {
            internal static Task RunTask(Task<T> task, Action<T> successor)
            {
                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                task.ContinueWith(delegate(Task<T> t)
                {
                    if (t.IsFaulted)
                    {
                        tcs.SetUnwrappedException(t.Exception);
                        return;
                    }
                    if (t.IsCanceled)
                    {
                        tcs.SetCanceled();
                        return;
                    }
                    try
                    {
                        successor(t.Result);
                        tcs.SetResult(null);
                    }
                    catch (Exception e)
                    {
                        tcs.SetUnwrappedException(e);
                    }
                });
                return tcs.Task;
            }
            internal static Task<TResult> RunTask(Task task, Func<TResult> successor)
            {
                TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
                task.ContinueWith(delegate(Task t)
                {
                    if (t.IsFaulted)
                    {
                        tcs.SetUnwrappedException(t.Exception);
                        return;
                    }
                    if (t.IsCanceled)
                    {
                        tcs.SetCanceled();
                        return;
                    }
                    try
                    {
                        tcs.SetResult(successor());
                    }
                    catch (Exception e)
                    {
                        tcs.SetUnwrappedException(e);
                    }
                });
                return tcs.Task;
            }
            internal static Task<TResult> RunTask(Task<T> task, Func<Task<T>, TResult> successor)
            {
                TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
                task.ContinueWith(delegate(Task<T> t)
                {
                    if (task.IsFaulted)
                    {
                        tcs.SetUnwrappedException(t.Exception);
                        return;
                    }
                    if (task.IsCanceled)
                    {
                        tcs.SetCanceled();
                        return;
                    }
                    try
                    {
                        tcs.SetResult(successor(t));
                    }
                    catch (Exception e)
                    {
                        tcs.SetUnwrappedException(e);
                    }
                });
                return tcs.Task;
            }
        }

        private static readonly Task EmptyTask = TaskHelper.FromResult<object>(null);

        public static Task Delay(TimeSpan timeOut)
        {
            TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
            Timer timer = new Timer(new TimerCallback(taskCompletionSource.SetResult), null, timeOut, TimeSpan.FromMilliseconds(-1.0));
            return taskCompletionSource.Task.ContinueWith(o =>
            {
                timer.Dispose();
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        public static Task<T> FromResult<T>(T value)
        {
            TaskCompletionSource<T> taskCompletionSource = new TaskCompletionSource<T>();
            taskCompletionSource.SetResult(value);
            return taskCompletionSource.Task;
        }

        public static Task FromMethod(Func<Task> func)
        {
            Task result;
            try
            {
                result = func();
            }
            catch (Exception e)
            {
                result = TaskHelper.FromResult<Exception>(e);
            }
            return result;
        }

        public static Task FromMethod(Action func)
        {
            Task result;
            try
            {
                func();
                result = TaskHelper.EmptyTask;
            }
            catch (Exception e)
            {
                result = TaskHelper.FromResult(e);
            }
            return result;
        }

        public static Task Then(this Task task, Func<Task> successor)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    return TaskHelper.FromMethod(successor);
                case TaskStatus.Canceled:
                case TaskStatus.Faulted:
                    return task;
                default:
                    return TaskHelper.TaskRunners<object, Task>.RunTask(task, successor).FastUnwrap();
            }
        }

        public static Task Then(this Task task, Action successor)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    return TaskHelper.FromMethod(successor);
                case TaskStatus.Canceled:
                case TaskStatus.Faulted:
                    return task;
                default:
                    return TaskHelper.RunTask(task, successor);
            }
        }

        private static Task RunTask(Task task, Action successor)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            task.ContinueWith( t=>
            {
                if (t.IsFaulted)
                {
                    tcs.SetUnwrappedException(t.Exception);
                    return;
                }
                if (t.IsCanceled)
                {
                    tcs.SetCanceled();
                    return;
                }
                try
                {
                    successor();
                    tcs.SetResult(null);
                }
                catch (Exception e)
                {
                    tcs.SetUnwrappedException(e);
                }
            });
            return tcs.Task;
        }

        public static Task FastUnwrap(this Task<Task> task)
        {
            Task task2 = (task.Status == TaskStatus.RanToCompletion) ? task.Result : null;
            return task2 ?? task.Unwrap();
        }

        internal static void SetUnwrappedException<T>(this TaskCompletionSource<T> tcs, Exception e)
        {
            AggregateException ex = e as AggregateException;
            if (ex != null)
            {
                tcs.SetException(ex.InnerExceptions);
                return;
            }
            tcs.SetException(e);
        }
    }
}
