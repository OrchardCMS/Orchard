using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.ServiceRuntime;
using Orchard.TaskLease.Services;

namespace Orchard.Azure.Services.TaskLease
{
	public class AzureMachineNameProvider : IMachineNameProvider
	{
		public string GetMachineName()
		{
			return RoleEnvironment.CurrentRoleInstance.Id;
		}
	}
}