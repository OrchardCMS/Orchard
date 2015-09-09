﻿namespace Orchard.Environment {

    /// <summary>
    /// Describes a service which returns the name of the machine running the application.
    /// </summary>
    public interface IMachineNameProvider {
    
        /// <summary>
        /// Returns the name of the machine running the application.
        /// </summary>
        string GetMachineName();
    }
}