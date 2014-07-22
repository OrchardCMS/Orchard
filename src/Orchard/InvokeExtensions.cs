using System;
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
}
