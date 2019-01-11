using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace MSBuildVersioning.Core
{
    /// <summary>
    /// Abstract class providing helper methods to execute and scrape information from source
    /// control command line programs.
    /// </summary>
    public abstract class SourceControlInfoProvider
    {
        protected SourceControlInfoProvider()
        {
            Path = String.Empty;
        }

        public string Path { get; set; }

        public abstract string SourceControlName { get; }

        public bool IgnoreToolNotFound { get; set; }

        protected virtual IList<string> ExecuteCommand(string fileName, string arguments)
        {
            return ExecuteCommand(fileName, arguments, null);
        }

        protected virtual IList<string> ExecuteCommand(string fileName, string arguments, Func<int, string, bool> errorHandler)
        {
            IList<string> output = new List<string>();
            ExecuteCommand(fileName, arguments, outputLine => output.Add(outputLine), errorHandler);
            return output;
        }

        protected virtual void ExecuteCommand(string fileName, string arguments,
            Action<string> outputHandler, Func<int, string, bool> errorHandler)
        {
            StringBuilder error = new StringBuilder();

            using (Process process = new Process())
            {
                process.StartInfo.FileName = fileName;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.WorkingDirectory = Path;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                process.OutputDataReceived += (o, e) =>
                    {
                        if (e.Data != null)
                            outputHandler(e.Data);
                    };

                process.ErrorDataReceived += (o, e) =>
                    {
                        if (e.Data != null)
                            error.AppendLine(e.Data);
                    };

                try
                {
                    process.Start();
                }
                catch (Win32Exception e)
                {
                    if (e.NativeErrorCode != 2 && e.NativeErrorCode != 267) // The system cannot find the file specified. || The directory name is invalid.
                    {
                        throw new BuildErrorException(String.Format(
                            "{0} command \"{1}\" with path \"{2}\" could not be started." + Environment.NewLine +
                            "Please ensure that {0} is installed. Error Code is {3}.",
                            SourceControlName, fileName, Path, e.NativeErrorCode));
                    }
                    if (IgnoreToolNotFound)
                    {
                        return;
                    }
                    throw new BuildErrorException(String.Format(
                        "{0} command \"{1}\" could not be found." + Environment.NewLine +
                        "Please ensure that {0} is installed. Error {2}.",
                        SourceControlName, fileName, e.NativeErrorCode));
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                var reportError = errorHandler != null && errorHandler(process.ExitCode, error.ToString());
                if (reportError && (process.ExitCode != 0 || error.Length > 0))
                {
                    throw new BuildErrorException(String.Format(
                        "{0} command \"{1} {2}\" exited with code {3}.\n{4}",
                        SourceControlName, fileName, arguments, process.ExitCode, error));
                }
            }
        }
    }
}
