﻿using System;
using System.Collections.Generic;
using System.Security;
using Orchard.Parameters;

namespace Orchard {
    public class OrchardParametersParser : IOrchardParametersParser {
        [SecurityCritical]
        public OrchardParameters Parse(CommandParameters parameters) {

            var result = new OrchardParameters {
                Arguments = new List<string>(),
                ResponseFiles = new List<string>(),
                Switches = new Dictionary<string, string>()
            };

            foreach (var arg in parameters.Arguments) {
                // @response-file
                if (arg[0] == '@') {
                    var filename = arg.Substring(1);
                    if (string.IsNullOrEmpty(filename)) {
                        throw new ArgumentException("Incorrect syntax: response file name can not be empty");
                    }
                    result.ResponseFiles.Add(filename);
                }
                // regular argument
                else {
                    result.Arguments.Add(arg);
                }
            }

            foreach (var sw in parameters.Switches) {
                // Built-in switches
                switch (sw.Key.ToLowerInvariant()) {
                    case "wd":
                    case "workingdirectory":
                        result.WorkingDirectory = sw.Value;
                        break;

                    case "v":
                    case "verbose":
                        bool verbose;
                        if (!bool.TryParse(sw.Value, out verbose))
                            verbose = true;
                        result.Verbose = verbose;
                        break;

                    case "vp":
                    case "virtualpath":
                        result.VirtualPath = sw.Value;
                        break;

                    case "t":
                    case "tenant":
                        result.Tenant = sw.Value;
                        break;

                    default:
                        result.Switches.Add(sw.Key, sw.Value);
                        break;
                }
            }

            return result;
        }
    }
}
