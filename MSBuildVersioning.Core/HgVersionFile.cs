namespace MSBuildVersioning.Core
{
    /// <summary>
    /// MSBuild task that reads a template file, replaces tokens in the file content with Mercurial
    /// versioning information for the project, and then writes the content to a destination file.
    /// </summary>
    public class HgVersionFile : VersionFile
    {
        public HgVersionFile()
            : base(new HgVersionTokenReplacer(new HgInfoProvider())) { }
    }
}
