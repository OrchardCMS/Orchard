using System;
using System.Timers;
using Autofac;
using Autofac.Integration.Web;
using Orchard.Environment;
using Orchard.Logging;

namespace Orchard.Tasks {
    public class SweepGenerator : IOrchardShellEvents {
        private readonly IContainer _container;
        private Timer _timer;

        public SweepGenerator(IContainer container) {
            _container = container;
            _timer = new Timer();
            _timer.Elapsed += Elapsed;
            Logger = NullLogger.Instance;
            Interval = TimeSpan.FromMinutes(5);
        }

        public ILogger Logger { get; set; }

        public TimeSpan Interval {
            get { return TimeSpan.FromMilliseconds(_timer.Interval); }
            set { _timer.Interval = value.TotalMilliseconds; }
        }

        public void Activated() {
            lock (_timer) {
                _timer.Start();
            }
        }

        public void Terminating() {
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
            // makes an inner container, similar to the per-request container

            var containerProvider = new ContainerProvider(_container);
            try {
                var requestContainer = containerProvider.RequestContainer;

                // also inject this instance in case anyone asks for the container provider
                requestContainer.Build(builder => builder.Register(containerProvider).As<IContainerProvider>());

                // resolve the manager and invoke it
                var manager = requestContainer.Resolve<IBackgroundService>();
                manager.Sweep();
            }
            finally{
                // shut everything down again
                containerProvider.DisposeRequestContainer();
            }
        }

        class ContainerProvider : IContainerProvider {
            public ContainerProvider(IContainer applicationContainer) {
                // explicitly create a request container for the life of this object
                ApplicationContainer = applicationContainer;
                RequestContainer = applicationContainer.CreateInnerContainer();
            }

            public void DisposeRequestContainer() {
                var disposeContainer = RequestContainer;
                RequestContainer = null;

                if (disposeContainer != null)
                    disposeContainer.Dispose();
            }

            public IContainer ApplicationContainer { get; private set; }

            public IContainer RequestContainer { get; private set; }
        }
    }
}
