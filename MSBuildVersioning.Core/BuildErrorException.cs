using System;

namespace MSBuildVersioning.Core
{
    /// <summary>
    /// Thrown when a build error occurs, and the message (but not the stacktrace) should be
    /// displayed in the Visual Studio "Error List" view.
    /// </summary>
    public class BuildErrorException : Exception
    {
        public BuildErrorException(string message)
            : base(message) { }
    }
}
