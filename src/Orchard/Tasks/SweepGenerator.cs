using System;
using System.Timers;
using Orchard.Data;
using Orchard.Logging;

namespace Orchard.Tasks {

    public interface ISweepGenerator : ISingletonDependency {
        void Activate();
        void Terminate();
    }

    public class SweepGenerator : ISweepGenerator, IDisposable {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly Timer _timer;

        public SweepGenerator(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
            _timer = new Timer();
            _timer.Elapsed += Elapsed;
            Logger = NullLogger.Instance;
            Interval = TimeSpan.FromMinutes(1);
        }

        public ILogger Logger { get; set; }

        public TimeSpan Interval {
            get { return TimeSpan.FromMilliseconds(_timer.Interval); }
            set { _timer.Interval = value.TotalMilliseconds; }
        }

        public void Activate() {
            lock (_timer) {
                _timer.Start();
            }
        }

        public void Terminate() {
            lock (_timer) {
                _timer.Stop();
            }
        }

        void Elapsed(object sender, ElapsedEventArgs e) {
            // current implementation disallows re-entrancy
            if (!System.Threading.Monitor.TryEnter(_timer))
                return;

            try {
                if (_timer.Enabled) {
                    DoWork();
                }
            }
            catch (Exception ex) {
                Logger.Warning(ex, "Problem in background tasks");
            }
            finally {
                System.Threading.Monitor.Exit(_timer);
            }
        }

        public void DoWork() {
            using (var scope = _workContextAccessor.CreateWorkContextScope()) {
                var transactionManager = scope.Resolve<ITransactionManager>();
                transactionManager.Demand();

                // resolve the manager and invoke it
                var manager = scope.Resolve<IBackgroundService>();
                manager.Sweep();
            }
        }

        public void Dispose() {
            _timer.Dispose();
        }
    }
}
