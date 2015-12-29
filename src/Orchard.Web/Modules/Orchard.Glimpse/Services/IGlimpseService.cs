using System;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Message;
using Orchard.Glimpse.Models;

namespace Orchard.Glimpse.Services {
    public interface IGlimpseService : IDependency {
        /// <summary>
        /// Executes an action, and times how long the execution takes
        /// </summary>
        /// <param name="action">The action to execute</param>
        /// <returns>An instance of TimerResult which contains the measured time taken to execute the action</returns>
        TimerResult Time(Action action);

        /// <summary>
        /// Executes an expression, times how long the execution takes, and returns the result of the timer and the result of the expression
        /// </summary>
        /// <typeparam name="T">The return type of the expression to execute</typeparam>
        /// <param name="action">The expression to execute</param>
        /// <returns>A TimedActionResult containing an instance of TimerResult depicting the measured time taken to execute the action, and the result of the action that was executed</returns>
        TimedActionResult<T> Time<T>(Func<T> action);

        /// <summary>
        /// Executes an action, times how long the action takes, and adds the result of the timed action to the Glimpse Timeline
        /// </summary>
        /// <param name="action">The action to execute</param>
        /// <param name="category">The <see cref="TimelineCategoryItem"/> that describes the way the action should be displayed on the Glimpse Timeline</param>
        /// <param name="eventName">The name of the event to be displayed on the Glimpse Timeline</param>
        /// <param name="eventSubText">Optional. A sub header to be displayed on the Glimpse Timeline that provides additional information about the action</param>
        /// <returns>A TimerResult depicting the measured time taken to execute the action</returns>
        TimerResult PublishTimedAction(Action action, TimelineCategoryItem category, string eventName, string eventSubText = null);

        /// <summary>
        /// Executes an action, times how long the action takes, adds the result of the timed action to the Glimpse Timeline, and adds an additional message to the Glimpse broker
        /// </summary>
        /// <typeparam name="TMessage">The type of the message to be added to the Glimpse broker</typeparam>
        /// <param name="action">The action to execute</param>
        /// <param name="messageFactory">An expression that returns the message to be added to the Glimpse broker</param>
        /// <param name="category">The <see cref="TimelineCategoryItem"/> that describes the way the action should be displayed on the Glimpse Timeline</param>
        /// <param name="eventName">The name of the event to be displayed on the Glimpse Timeline</param>
        /// <param name="eventSubText">Optional. A sub header to be displayed on the Glimpse Timeline that provides additional information about the action</param>
        /// <returns>A TimerResult depicting the measured time taken to execute the action</returns>
        TimerResult PublishTimedAction<TMessage>(Action action, Func<TMessage> messageFactory, TimelineCategoryItem category, string eventName, string eventSubText = null) where TMessage : class;

        /// <summary>
        /// Executes an action, times how long the action takes, adds the result of the timed action to the Glimpse Timeline, and adds an additional message to the Glimpse broker
        /// </summary>
        /// <typeparam name="TMessage">The type of the message to be added to the Glimpse broker</typeparam>
        /// <param name="action">The action to execute</param>
        /// <param name="messageFactory">An expression that returns the message to be added to the Glimpse broker. The expression will be passed an instance of <see cref="TimerResult"/> that contains data about the time take to execute <paramref name="action"/></param>
        /// <param name="category">The <see cref="TimelineCategoryItem"/> that describes the way the action should be displayed on the Glimpse Timeline</param>
        /// <param name="eventName">The name of the event to be displayed on the Glimpse Timeline</param>
        /// <param name="eventSubText">Optional. A sub header to be displayed on the Glimpse Timeline that provides additional information about the action</param>
        /// <returns>A TimerResult depicting the measured time taken to execute the action</returns>
        TimerResult PublishTimedAction<TMessage>(Action action, Func<TimerResult, TMessage> messageFactory, TimelineCategoryItem category, string eventName, string eventSubText = null) where TMessage : class;

        /// <summary>
        /// Executes an expression, times how long the expression takes, and adds the result of the timed expression to the Glimpse Timeline
        /// </summary>
        /// <typeparam name="T">The return type of the expression to execute</typeparam>
        /// <param name="action">The expression to execute</param>
        /// <param name="category">The <see cref="TimelineCategoryItem"/> that describes the way the action should be displayed on the Glimpse Timeline</param>
        /// <param name="eventName">The name of the event to be displayed on the Glimpse Timeline</param>
        /// <param name="eventSubText">Optional. A sub header to be displayed on the Glimpse Timeline that provides additional information about the expression</param>
        /// <returns>A TimedActionResult containing an instance of TimerResult depicting the measured time taken to execute the expression, and the result of the expression that was executed</returns>
        TimedActionResult<T> PublishTimedAction<T>(Func<T> action, TimelineCategoryItem category, string eventName, string eventSubText = null, Func<T, bool> publishCondition = null);

        TimedActionResult<T> PublishTimedAction<T>(Func<T> action, TimelineCategoryItem category, Func<T, string> eventNameFactory, Func<T, string> eventSubTextFactory = null, Func<T, bool> publishCondition = null);

        TimedActionResult<T> PublishTimedAction<T, TMessage>(Func<T> action, Func<T, TimerResult, TMessage> messageFactory, TimelineCategoryItem category, string eventName, string eventSubText = null, Func<T, bool> publishCondition = null) where TMessage : class;

        TimedActionResult<T> PublishTimedAction<T, TMessage>(Func<T> action, Func<T, TimerResult, TMessage> messageFactory, TimelineCategoryItem category, Func<T, string> eventNameFactory, Func<T, string> eventSubTextFactory = null, Func<T, bool> publishCondition = null) where TMessage : class;

        /// <summary>
        /// Places a message into the Glimpse Broker
        /// </summary>
        /// <typeparam name="T">The type of the message to be published</typeparam>
        /// <param name="message">The message to be published</param>
        void PublishMessage<T>(T message) where T : class;
    }
}