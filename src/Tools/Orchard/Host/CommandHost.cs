using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Security;
using System.Web.Hosting;
using Orchard.Parameters;
using Orchard.ResponseFiles;

namespace Orchard.Host {

    /// <summary>
    /// Different return codes for a command execution.
    /// </summary>
    public enum CommandReturnCodes {
        Ok = 0,
        Fail = 5,
        Retry = 240
    }

    /// <summary>
    /// The CommandHost runs inside the ASP.NET AppDomain and serves as an intermediate
    /// between the command line and the CommandHostAgent, which is known to the Orchard
    /// Framework and has the ability to execute commands.
    /// </summary>
    public class CommandHost : MarshalByRefObject, IRegisteredObject {
        private object _agent;

        public CommandHost() {
            HostingEnvironment.RegisterObject(this);
        }

        [SecurityCritical]
        public override object InitializeLifetimeService() {
            // never expire the cross-AppDomain lease on this object
            return null;
        }

        private static void ExtendLifeTimeLeases(TextReader input, TextWriter output) {
            // Orchard objects passed as parameters into this AppDomain should derive from MarshalByRefObject and have
            // infinite lease timeouts by means of their InitializeLifetimeService overrides.  For the input/output 
            // stream objects we approximate that behavior by immediately renewing the lease for 30 days.
            ExtendLifeTimeLease(input);
            ExtendLifeTimeLease(output);
        }

        private static void ExtendLifeTimeLease(MarshalByRefObject obj) {
            if (RemotingServices.IsObjectOutOfAppDomain(obj)) {
                var lease = (ILease)RemotingServices.GetLifetimeService(obj);
                lease.Renew(TimeSpan.FromDays(30));
            }
        }

        [SecuritySafeCritical]
        void IRegisteredObject.Stop(bool immediate) {
            HostingEnvironment.UnregisterObject(this);
        }

        public CommandReturnCodes StartSession(TextReader input, TextWriter output) {
            ExtendLifeTimeLeases(input, output);
            _agent = CreateAgent();
            return StartHost(_agent, input, output);
        }

        public void StopSession(TextReader input, TextWriter output) {
            if (_agent != null) {
                ExtendLifeTimeLeases(input, output);
                StopHost(_agent, input, output);
                _agent = null;
            }
        }

        public CommandReturnCodes RunCommand(TextReader input, TextWriter output, Logger logger, OrchardParameters args) {
            ExtendLifeTimeLeases(input, output);
            var agent = CreateAgent();
            CommandReturnCodes result = (CommandReturnCodes)agent.GetType().GetMethod("RunSingleCommand").Invoke(agent, new object[] { 
                input,
                output,
                args.Tenant,
                args.Arguments.ToArray(),
                args.Switches});

            return result;
        }

        public CommandReturnCodes RunCommandInSession(TextReader input, TextWriter output, Logger logger, OrchardParameters args) {
            ExtendLifeTimeLeases(input, output);
            CommandReturnCodes result = (CommandReturnCodes)_agent.GetType().GetMethod("RunCommand").Invoke(_agent, new object[] { 
                input,
                output,
                args.Tenant,
                args.Arguments.ToArray(),
                args.Switches});

            return result;
        }

        public CommandReturnCodes RunCommands(TextReader input, TextWriter output, Logger logger, IEnumerable<ResponseLine> responseLines) {
            ExtendLifeTimeLeases(input, output);
            var agent = CreateAgent();

            CommandReturnCodes result = StartHost(agent, input, output);
            if (result != CommandReturnCodes.Ok)
                return result;

            foreach (var line in responseLines) {
                logger.LogInfo("{0} ({1}): Running command: {2}", line.Filename, line.LineNumber, line.LineText);

                var args = new OrchardParametersParser().Parse(new CommandParametersParser().Parse(line.Args));

                result = (CommandReturnCodes)agent.GetType().GetMethod("RunCommand").Invoke(agent, new object[] { 
                    input,
                    output,
                    args.Tenant,
                    args.Arguments.ToArray(),
                    args.Switches});

                if (result != CommandReturnCodes.Ok) {
                    output.WriteLine("{0} ({1}): Command returned error ({2})", line.Filename, line.LineNumber, result);
                    return result;
                }
            }

            result = StopHost(agent, input, output);
            return result;
        }

        private object CreateAgent() {
            return Activator.CreateInstance("Orchard.Framework", "Orchard.Commands.CommandHostAgent").Unwrap();
        }

        private CommandReturnCodes StopHost(object agent, TextReader input, TextWriter output) {
            return (CommandReturnCodes)agent.GetType().GetMethod("StopHost").Invoke(agent, new object[] { input, output });
        }

        private CommandReturnCodes StartHost(object agent, TextReader input, TextWriter output) {
            return (CommandReturnCodes)agent.GetType().GetMethod("StartHost").Invoke(agent, new object[] { input, output });
        }
    }
}
