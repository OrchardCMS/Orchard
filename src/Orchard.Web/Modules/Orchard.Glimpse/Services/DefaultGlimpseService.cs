using System;
using System.Collections.Generic;
using System.Web;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Framework;
using Glimpse.Core.Message;
using Orchard.Glimpse.Interceptors;
using Orchard.Glimpse.Models;
using Orchard.Glimpse.Tabs;
using Orchard.Localization;
using ILogger = Orchard.Logging.ILogger;
using NullLogger = Orchard.Logging.NullLogger;

namespace Orchard.Glimpse.Services {
    public class DefaultGlimpseService : IGlimpseService {
        private readonly IEnumerable<IGlimpseMessageInterceptor> _messageInterceptors;

        public DefaultGlimpseService(IEnumerable<IGlimpseMessageInterceptor> messageInterceptors) {
            _messageInterceptors = messageInterceptors;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; private set; }

        public TimerResult Time(Action action) {
            var executionTimer = GetTimer();

            if (executionTimer == null) {
                action();
                return new TimerResult();
            }

            return executionTimer.Time(action);
        }

        public TimedActionResult<T> Time<T>(Func<T> action) {
            var result = default(T);

            var executionTimer = GetTimer();

            if (executionTimer == null) {
                return new TimedActionResult<T> {ActionResult = action() };
            }

            var duration = executionTimer.Time(() => { result = action(); });

            return new TimedActionResult<T> {
                ActionResult = result,
                TimerResult = duration
            };
        }

        public TimerResult PublishTimedAction(Action action, TimelineCategoryItem category, string eventName, string eventSubText = null) {
            var timedResult = Time(action);
            PublishMessage(new TimelineMessage {EventName = eventName, EventCategory = category, EventSubText = eventSubText}.AsTimedMessage(timedResult));

            return timedResult;
        }

        public TimerResult PublishTimedAction<TMessage>(Action action, Func<TMessage> messageFactory, TimelineCategoryItem category, string eventName, string eventSubText = null) where TMessage : class {
            var timedResult = PublishTimedAction(action, category, eventName, eventSubText);
            PublishMessage(messageFactory());

            return timedResult;
        }

        public TimerResult PublishTimedAction<TMessage>(Action action, Func<TimerResult, TMessage> messageFactory, TimelineCategoryItem category, string eventName, string eventSubText = null) where TMessage : class {
            var timedResult = PublishTimedAction(action, category, eventName, eventSubText);
            PublishMessage(messageFactory(timedResult));

            return timedResult;
        }

        public TimedActionResult<T> PublishTimedAction<T>(Func<T> action, TimelineCategoryItem category, string eventName, string eventSubText = null, Func<T, bool> publishCondition = null) {
            var timedResult = Time(action);

            if (publishCondition==null || publishCondition(timedResult.ActionResult)) {
                PublishMessage(new TimelineMessage {EventName = eventName, EventCategory = category, EventSubText = eventSubText}.AsTimedMessage(timedResult.TimerResult));
            }

            return timedResult;
        }

        public TimedActionResult<T> PublishTimedAction<T>(Func<T> action, TimelineCategoryItem category, Func<T, string> eventNameFactory, Func<T, string> eventSubTextFactory = null, Func<T, bool> publishCondition = null) {
            var timedResult = Time(action);

            string eventSubText = null;
            if (eventSubTextFactory != null) {
                eventSubText = eventSubTextFactory(timedResult.ActionResult);
            }

            if (publishCondition == null || publishCondition(timedResult.ActionResult)) {
                PublishMessage(new TimelineMessage {EventName = eventNameFactory(timedResult.ActionResult), EventCategory = category, EventSubText = eventSubText}.AsTimedMessage(timedResult.TimerResult));
            }

            return timedResult;
        }

        public TimedActionResult<T> PublishTimedAction<T, TMessage>(Func<T> action, Func<T, TimerResult, TMessage> messageFactory, TimelineCategoryItem category, string eventName, string eventSubText = null, Func<T, bool> publishCondition = null) where TMessage : class {
            var actionResult = PublishTimedAction(action, category, eventName, eventSubText);

            if (publishCondition == null || publishCondition(actionResult.ActionResult)) {
                PublishMessage(messageFactory(actionResult.ActionResult, actionResult.TimerResult));
            }

            return actionResult;
        }

        public TimedActionResult<T> PublishTimedAction<T, TMessage>(Func<T> action, Func<T, TimerResult, TMessage> messageFactory, TimelineCategoryItem category, Func<T, string> eventNameFactory, Func<T, string> eventSubTextFactory = null, Func<T, bool> publishCondition = null) where TMessage : class {
            var actionResult = PublishTimedAction(action, category, eventNameFactory, eventSubTextFactory);

            if (publishCondition == null || publishCondition(actionResult.ActionResult)) {
                PublishMessage(messageFactory(actionResult.ActionResult, actionResult.TimerResult));
            }

            return actionResult;
        }

        public void PublishMessage<T>(T message) where T : class {
            var broker = GetMessageBroker();

            _messageInterceptors.Invoke(i => i.MessageReceived(message), Logger);

            broker?.Publish(message);
        }

        private IExecutionTimer GetTimer() {
            var context = HttpContext.Current;

            return ((GlimpseRuntime) context?.Application.Get("__GlimpseRuntime"))?.Configuration.TimerStrategy.Invoke();
        }

        private IMessageBroker GetMessageBroker() {
            var context = HttpContext.Current;

            return ((GlimpseRuntime) context?.Application.Get("__GlimpseRuntime"))?.Configuration.MessageBroker;
        }
    }

    public class NullMessageBroker : IMessageBroker {
        public void Publish<T>(T message) { }

        public Guid Subscribe<T>(Action<T> action) {
            return Guid.NewGuid();
        }

        public void Unsubscribe<T>(Guid subscriptionId) { }
    }
}