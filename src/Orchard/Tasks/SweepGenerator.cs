using System;
using System.Runtime.InteropServices;
using System.Timers;
using System.Web.Hosting;
using Orchard.Logging;

namespace Orchard.Tasks {

    public interface ISweepGenerator : IRegisteredObject, ISingletonDependency {
        void Activate();
        void Terminate();
    }

    public class SweepGenerator : ISweepGenerator, IDisposable {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly Timer _timer;
        private bool _shuttingDown;

        private IBackgroundService _manager;

        public SweepGenerator(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
            _timer = new Timer();
            _timer.Elapsed += Elapsed;
            Logger = NullLogger.Instance;
            Interval = TimeSpan.FromMinutes(1);

            HostingEnvironment.RegisterObject(this);
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
                if (_timer.Enabled && !_shuttingDown) {
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
                // resolve the manager and invoke it
                _manager = scope.Resolve<IBackgroundService>();
                _manager.Sweep();
            }
        }

        public void Dispose() {
            _timer.Dispose();
        }

        public void Stop(bool immediate) {
            _shuttingDown = true;
            _manager.Terminate();

            HostingEnvironment.UnregisterObject(this); 
        }
    }
}
