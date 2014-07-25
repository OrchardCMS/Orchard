using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Orchard.Security;
using Orchard.Logging;
using Orchard.Exceptions;

namespace Orchard {

    public static class InvokeExtensions {

        /// <summary>
        /// Safely invoke methods by catching non fatal exceptions and logging them
        /// </summary>
        public static void Invoke<TEvents>(this IEnumerable<TEvents> events, Action<TEvents> dispatch, ILogger logger) {
            foreach (var sink in events) {
                try {
                    dispatch(sink);
                } catch (Exception ex) {
                    if (IsLogged(ex)) {
                        logger.Error(ex, "{2} thrown from {0} by {1}",
                            typeof(TEvents).Name,
                            sink.GetType().FullName,
                            ex.GetType().Name);
                    }

                    if (ex.IsFatal()) {
                        throw;
                    }
                }
            }
        }

        public static IEnumerable<TResult> Invoke<TEvents, TResult>(this IEnumerable<TEvents> events, Func<TEvents, TResult> dispatch, ILogger logger) {

            foreach (var sink in events) {
                TResult result = default(TResult);
                try {
                    result = dispatch(sink);
                } catch (Exception ex) {
                    if (IsLogged(ex)) {
                        logger.Error(ex, "{2} thrown from {0} by {1}",
                            typeof(TEvents).Name,
                            sink.GetType().FullName,
                            ex.GetType().Name);
                    }

                    if (ex.IsFatal()) {
                        throw;
                    }
                }

                yield return result;
            }
        }

        /// <summary>
        /// Safely invoke methods asynchronously by catching non fatal exceptions and logging them
        /// </summary>
        public static Task InvokeAsync<TEvents>(this IEnumerable<TEvents> events, Func<TEvents, Task> dispatch, ILogger logger) {

            return Task.WhenAll(events.Select(async sink => {
                try {
                    await dispatch(sink);
                } catch (Exception ex) {
                    if (IsLogged(ex)) {
                        logger.Error(ex, "{2} thrown from {0} by {1}",
                            typeof(TEvents).Name,
                            sink.GetType().FullName,
                            ex.GetType().Name);
                    }

                    if (ex.IsFatal()) {
                        throw;
                    }
                }
            }).ToList());
        }

        public static IEnumerable<Task<TResult>> InvokeAsync<TEvents, TResult>(this IEnumerable<TEvents> events, Func<TEvents, Task<TResult>> dispatch, ILogger logger) {
            return events.Select(async sink => {
                var result = default(TResult);

                try {
                    result = await dispatch(sink);
                } catch (Exception ex) {
                    if (IsLogged(ex)) {
                        logger.Error(ex, "{2} thrown from {0} by {1}",
                            typeof(TEvents).Name,
                            sink.GetType().FullName,
                            ex.GetType().Name);
                    }

                    if (ex.IsFatal()) {
                        throw;
                    }
                }

                return result;
            }).ToList();
        }

        /// <summary>
        /// Returns a sequence of tasks which will be observed to complete with the same set
        /// of results as the given input tasks, but in the order in which the original tasks complete.
        /// </summary>
        /// <remarks>
        /// This code is from Jon Skeet's excellent edusync project on google code
        /// </remarks>
        public static IEnumerable<Task<T>> InCompletionOrder<T>(this IEnumerable<Task<T>> source) {

            var inputs = source.ToList();
            var boxes = inputs.Select(x => new TaskCompletionSource<T>()).ToArray();

            var currentIndex = -1;

            foreach (var task in inputs) {
                task.ContinueWith(completed => {
                    var box = boxes[Interlocked.Increment(ref currentIndex)];
                    completed.PropagateResult(box);
                }, TaskContinuationOptions.ExecuteSynchronously);
            }

            return boxes.Select(box => box.Task);
        }

        /// <summary>
        /// Propagates the status of the given task (which must be completed) to a task completion source (which should not be).
        /// </summary>
        private static void PropagateResult<T>(this Task<T> completedTask, TaskCompletionSource<T> completionSource) {
            switch (completedTask.Status) {
                case TaskStatus.Canceled:
                    completionSource.TrySetCanceled();
                    break;
                case TaskStatus.Faulted:
                    completionSource.TrySetException(completedTask.Exception.InnerExceptions);
                    break;
                case TaskStatus.RanToCompletion:
                    completionSource.TrySetResult(completedTask.Result);
                    break;
                default:
                    throw new ArgumentException("Task was not completed");
            }
        }

        private static bool IsLogged(Exception ex) {
            return ex is OrchardSecurityException || !ex.IsFatal();
        }
    }


    /// <summary>
    /// A Helper class to run Asynchronous functions from synchronous ones. Source: https://github.com/OmerMor/AsyncBridge
    /// </summary>
    public static class AsyncHelper
    {
        /// <summary>
        /// A class to bridge synchronous asynchronous methods
        /// </summary>
        public class AsyncBridge : IDisposable
        {
            private ExclusiveSynchronizationContext CurrentContext;
            private SynchronizationContext OldContext;
            private int TaskCount;

            /// <summary>
            /// Constructs the AsyncBridge by capturing the current
            /// SynchronizationContext and replacing it with a new
            /// ExclusiveSynchronizationContext.
            /// </summary>
            internal AsyncBridge()
            {
                OldContext = SynchronizationContext.Current;
                CurrentContext = new ExclusiveSynchronizationContext(OldContext);
                SynchronizationContext.SetSynchronizationContext(CurrentContext);
            }

            /// <summary>
            /// Executes an async task with a void return type
            /// from a synchronous context
            /// </summary>
            /// <param name="taskFactory">Task Factory to execute</param>
            /// <param name="callback">Optional callback</param>
            public void Run(Func<Task> taskFactory, Action<Task> callback = null)
            {
                CurrentContext.Post(async _ =>
                {
                    try
                    {
                        Increment();

                        var task = taskFactory();
                        await task;

                        if (null != callback)
                        {
                            callback(task);
                        }
                    }
                    catch (Exception e)
                    {
                        CurrentContext.InnerException = e;
                    }
                    finally
                    {
                        Decrement();
                    }
                }, null);
            }

            /// <summary>
            /// Executes an async task with a void return type
            /// from a synchronous context
            /// </summary>
            /// <param name="task">Task to execute</param>
            /// <param name="callback">Optional callback</param>
            public void Run(Task task, Action<Task> callback = null)
            {
                Run(() => task, callback);
            }

            /// <summary>
            /// Executes an async task with a T return type
            /// from a synchronous context
            /// </summary>
            /// <typeparam name="T">The type of the task</typeparam>
            /// <param name="taskFactory">Task Factory to execute</param>
            /// <param name="callback">Optional callback</param>
            public void Result<T>(Func<Task<T>> taskFactory, Action<Task<T>> callback = null)
            {
                if (callback == null)
                    Run(taskFactory);
                else
                    Run(taskFactory, finishedTask => callback((Task<T>)finishedTask));
            }

            /// <summary>
            /// Executes an async task with a T return type
            /// from a synchronous context
            /// </summary>
            /// <typeparam name="T">The type of the task</typeparam>
            /// <param name="taskFactory">Task Factory to execute</param>
            /// <param name="callback">
            /// The callback function that uses the result of the task
            /// </param>
            public void Result<T>(Func<Task<T>> taskFactory, Action<T> callback)
            {
                Result(taskFactory, t => callback(t.Result));
            }

            /// <summary>
            /// Executes an async task with a T return type
            /// from a synchronous context
            /// </summary>
            /// <typeparam name="T">The type of the task</typeparam>
            /// <param name="task">Task to execute</param>
            /// <param name="callback">
            /// The callback function that uses the result of the task
            /// </param>
            public void Result<T>(Task<T> task, Action<T> callback)
            {
                Result(() => task, callback);
            }

            /// <summary>
            /// Executes an async task with a T return type
            /// from a synchronous context
            /// </summary>
            /// <typeparam name="T">The type of the task</typeparam>
            /// <param name="task">Task to execute</param>
            /// <param name="callback">Optional callback</param>
            public void Result<T>(Task<T> task, Action<Task<T>> callback = null)
            {
                Result(() => task, callback);
            }

            private void Increment()
            {
                Interlocked.Increment(ref TaskCount);
            }

            private void Decrement()
            {
                Interlocked.Decrement(ref TaskCount);
                if (TaskCount == 0)
                {
                    CurrentContext.EndMessageLoop();
                }
            }

            /// <summary>
            /// Disposes the object
            /// </summary>
            public void Dispose()
            {
                try
                {
                    CurrentContext.BeginMessageLoop();
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(OldContext);
                }
            }
        }

        /// <summary>
        /// Creates a new AsyncBridge. This should always be used in
        /// conjunction with the using statement, to ensure it is disposed
        /// </summary>
        public static AsyncBridge Wait
        {
            get { return new AsyncBridge(); }
        }

        /// <summary>
        /// Runs a task with the "Fire and Forget" pattern using Task.Run,
        /// and unwraps and handles exceptions
        /// </summary>
        /// <param name="task">A function that returns the task to run</param>
        /// <param name="handle">Error handling action, null by default</param>
        public static void FireAndForget(
            Func<Task> task,
            Action<Exception> handle = null)
        {
            Task.Run(
            () =>
            {
                ((Func<Task>)(async () =>
                {
                    try
                    {
                        await task();
                    }
                    catch (Exception e)
                    {
                        if (null != handle)
                        {
                            handle(e);
                        }
                    }
                }))();
            });
        }

        private class ExclusiveSynchronizationContext : SynchronizationContext
        {
            private readonly AutoResetEvent _workItemsWaiting =
                new AutoResetEvent(false);

            private bool _done;
            private ConcurrentQueue<Tuple<SendOrPostCallback, object>> _items;

            public Exception InnerException { get; set; }

            public ExclusiveSynchronizationContext(SynchronizationContext old)
            {
                var oldEx = old as ExclusiveSynchronizationContext;

                _items = null != oldEx ? oldEx._items : new ConcurrentQueue<Tuple<SendOrPostCallback, object>>();
            }

            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException(
                    "We cannot send to our same thread");
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                _items.Enqueue(Tuple.Create(d, state));
                _workItemsWaiting.Set();
            }

            public void EndMessageLoop()
            {
                Post(_ => _done = true, null);
            }

            public void BeginMessageLoop()
            {
                while (!_done)
                {
                    Tuple<SendOrPostCallback, object> task;

                    if (!_items.TryDequeue(out task))
                    {
                        task = null;
                    }

                    if (task != null)
                    {
                        task.Item1(task.Item2);
                        if (InnerException != null) // method threw an exception
                        {
                            throw new AggregateException("AsyncBridge.Run method threw an exception.", InnerException);
                        }
                    }
                    else
                    {
                        _workItemsWaiting.WaitOne();
                    }
                }
            }

            public override SynchronizationContext CreateCopy()
            {
                return this;
            }
        }
    }
}
