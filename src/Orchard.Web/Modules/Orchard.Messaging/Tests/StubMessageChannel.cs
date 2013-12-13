using System;
using Orchard.Messaging.Models;
using Orchard.Messaging.Services;
using Orchard.Tests.Stubs;

namespace Orchard.Messaging.Tests {
    public class StubMessageChannel : IMessageChannel {
        public const string ChannelName = "stub";
        private readonly TimeSpan _simulatedProcessingTime;
        private readonly StubClock _clock;

        public StubMessageChannel(TimeSpan simulatedProcessingTime, StubClock clock) {
            _simulatedProcessingTime = simulatedProcessingTime;
            _clock = clock;
        }

        public void Dispose() {
        }

        public string Name { get { return ChannelName; } }

        public void Send(QueuedMessage message) {
            _clock.Advance(_simulatedProcessingTime);
        }
    }
}