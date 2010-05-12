using System;
using Orchard.Events;
using Orchard.Environment.Configuration;
using Orchard.Environment;

namespace Orchard.MultiTenancy.Services {
    public class ShellSettingsEventHandler : IShellSettingsEventHandler, IDependency {

        private readonly IRunningShellTable _runningShellTable;

        public ShellSettingsEventHandler(IRunningShellTable runningShellTable) {
            _runningShellTable = runningShellTable;        
        }

        public void Created(ShellSettings settings) {
            _runningShellTable.Add(settings);
        }

        public void Deleted(ShellSettings settings) {
            _runningShellTable.Remove(settings);
        }

        public void Updated(ShellSettings settings) {
            _runningShellTable.Update(settings);
        }
    }
}
