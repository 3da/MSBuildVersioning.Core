using System.Globalization;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuildVersioning.Core
{
    public class HgVersionInformation : Task
    {
        public bool IgnoreToolNotFound { get; set; }

        public string ToolPath { get; set; }

        [Output]
        public string Revision { get; set; }

        [Output]
        public string Dirty { get; set; }

        public override bool Execute()
        {
            try
            {
                HgInfoProvider infoProvider = new HgInfoProvider();
                infoProvider.Path = ToolPath;
                infoProvider.IgnoreToolNotFound = IgnoreToolNotFound;

                Revision = infoProvider.GetRevisionNumber().ToString(CultureInfo.InvariantCulture);
                Dirty = infoProvider.IsWorkingCopyDirty() ? "1" : "0";
                return true;
            }
            catch (BuildErrorException e)
            {
                Log.LogError(e.Message);
                return false;
            }
        }
    }
}