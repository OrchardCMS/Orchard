using System;
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

        public void Process(string payload) {
            _clock.Advance(_simulatedProcessingTime);
        }
    }
}