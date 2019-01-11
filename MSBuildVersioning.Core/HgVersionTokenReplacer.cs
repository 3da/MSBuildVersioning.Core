namespace MSBuildVersioning.Core
{
    /// <summary>
    /// Replaces tokens in a string with information from a <c>HgInfoProvider</c>.
    /// </summary>
    public class HgVersionTokenReplacer : VersionTokenReplacer
    {
        public HgVersionTokenReplacer(HgInfoProvider infoProvider)
        {
            SourceControlInfoProvider = infoProvider;

            AddToken("REVNUM", () => infoProvider.GetRevisionNumber().ToString());
            AddToken("REVNUM_MOD", x => (infoProvider.GetRevisionNumber() % x).ToString());
            AddToken("REVNUM_DIV", x => (infoProvider.GetRevisionNumber() / x).ToString());
            AddToken("REVID", () => infoProvider.GetRevisionId());
            AddToken("REVIDLONG", () => infoProvider.GetLongRevisionId());
            AddToken("DIRTY", () => infoProvider.IsWorkingCopyDirty() ? "1" : "0");
            AddToken("BRANCH", () => infoProvider.GetBranch());
            AddToken("TAGS", () => infoProvider.GetTags());
        }
    }
}
