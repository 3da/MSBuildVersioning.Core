using System;
using System.Collections.Generic;
using System.IO;

namespace MSBuildVersioning.Core
{
    /// <summary>
    /// Provides Mercurial information for a particular file path, by executing and scraping
    /// information from the hg.exe command-line program.
    /// </summary>
    public class HgInfoProvider : SourceControlInfoProvider
    {
        private int? _revisionNumber;
        private string _revisionId;
        private string _longRevisionId;
        private bool? _isWorkingCopyDirty;
        private string _branch;
        private string _tags;
        private string _changeSetDate;

        public override string SourceControlName
        {
            get { return "Mercurial"; }
        }

        public virtual int GetRevisionNumber()
        {
            if (_revisionNumber == null)
            {
                _revisionNumber = int.Parse(ExecuteRevisionCommand("identify -n"));
            }
            return (int)_revisionNumber;
        }

        public virtual string GetRevisionId()
        {
            if (_revisionId == null)
            {
                _revisionId = ExecuteRevisionCommand("identify -i");
            }
            return _revisionId;
        }

        public virtual string GetLongRevisionId()
        {
            if (_longRevisionId == null)
            {
                _longRevisionId = ExecuteRevisionCommand("identify -i --debug");
            }
            return _longRevisionId;
        }

        private string ExecuteRevisionCommand(string hgArguments)
        {
            IList<string> results = ExecuteCommand("hg.exe", hgArguments);
            if (results.Count == 0)
            {
                _isWorkingCopyDirty = true;
                return "0";
            }
            string result = results[0];

            if (result.Contains("+"))
            {
                _isWorkingCopyDirty = true;
                result = result.Substring(0, result.IndexOf("+"));
            }
            else
            {
                _isWorkingCopyDirty = false;
            }

            return result;
        }

        public virtual bool IsWorkingCopyDirty()
        {
            if (_isWorkingCopyDirty == null)
            {
                GetRevisionNumber();
            }
            return (bool)_isWorkingCopyDirty;
        }

        public virtual string GetBranch()
        {
            if (_branch == null)
            {
                _branch = ExecuteCommand("hg.exe", "identify -b")[0];
            }
            return _branch;
        }

        public virtual string GetTags()
        {
            if (_tags == null)
            {
                _tags = ExecuteCommand("hg.exe", "identify -t")[0];
            }
            return _tags;
        }

        public virtual string GetChangesetDate()
        {
            if (_changeSetDate == null)
            {
                var result =
                    ExecuteCommand("hg.exe", $"log --template \"{{date|isodate}}\" -r{GetRevisionNumber()}");

                _changeSetDate = result[1];
            }

            return _changeSetDate;
        }
    }
}
