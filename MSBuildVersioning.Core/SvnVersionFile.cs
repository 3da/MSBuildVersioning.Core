namespace MSBuildVersioning.Core
{
    /// <summary>
    /// MSBuild task that reads a template file, replaces tokens in the file content with Subversion
    /// versioning information for the project, and then writes the content to a destination file.
    /// </summary>
    public class SvnVersionFile : VersionFile
    {
        public SvnVersionFile()
            : base(new SvnVersionTokenReplacer(new SvnInfoProvider())) { }
    }
}
