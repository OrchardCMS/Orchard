using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.ServiceRuntime;
using Orchard.Environment;

namespace Orchard.Azure.Services.TaskLease {
    public class AzureMachineNameProvider : IMachineNameProvider {
        public string GetMachineName() {
            return RoleEnvironment.CurrentRoleInstance.Id;
        }
    }
}