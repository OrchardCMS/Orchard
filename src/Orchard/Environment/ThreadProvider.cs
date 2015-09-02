using System.Threading;

namespace Orchard.Environment {
    public class ThreadProvider : IThreadProvider {
        public int GetCurrentThreadId() {
            return Thread.CurrentThread.ManagedThreadId;
        }
    }
}