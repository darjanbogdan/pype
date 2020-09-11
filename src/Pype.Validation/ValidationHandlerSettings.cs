using System;
using System.Collections.Generic;
using System.Text;

namespace Pype.Validation
{
    /// <summary>
    /// Holds the configuration of a validation decorator
    /// </summary>
    public class ValidationHandlerSettings
    {

        /// <summary>
        /// Gets or sets a value indicating whether validation should stop on first failure or continue to process all validators.
        /// </summary>
        /// <value>
        ///   <c>true</c> [Default] if validator stops on the first failure; otherwise <c>false</c>.
        /// </value>
        public bool StopOnFailure { get; set; } = true;
    }
}
