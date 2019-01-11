using System;

namespace MSBuildVersioning.Core
{
    /// <summary>
    /// Provides Mercurial information for a particular file path, by executing and scraping
    /// information from the hg.exe command-line program.
    /// </summary>
    public class GitInfoProvider : SourceControlInfoProvider
    {
        private int? _revisionNumber;
        private string _revisionId;
        private bool? _isWorkingCopyDirty;
        private string _branch;
        private string _tags;

        public override string SourceControlName
        {
            get { return "Git"; }
        }

        public virtual int GetRevisionNumber()
        {
            if (_revisionNumber == null)
            {
                InitRevision();
            }
            return (int)_revisionNumber;
        }

        public virtual string GetRevisionId()
        {
            if (_revisionId == null)
            {
                InitRevision();
            }
            return _revisionId;
        }

        public object GetLongRevisionId()
        {
            throw new NotImplementedException();
        }

        private void InitRevision()
        {
            ExecuteCommand("git.exe", "rev-list HEAD", output =>
            {
                if (_revisionId == null)
                {
                    _revisionId = output;
                    _revisionNumber = 1;
                }
                else
                {
                    _revisionNumber += 1;
                }
            },
            null);
        }

        public virtual bool IsWorkingCopyDirty()
        {
            if (_isWorkingCopyDirty == null)
            {
                ExecuteCommand("git.exe", "diff-index --quiet HEAD", (exitCode, error) =>
                {
                    if (exitCode == 0)
                    {
                        _isWorkingCopyDirty = false;
                        return false;
                    }
                    else if (exitCode == 1)
                    {
                        _isWorkingCopyDirty = true;
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                });
            }
            return (bool)_isWorkingCopyDirty;
        }

        public virtual string GetBranch()
        {
            if (_branch == null)
            {
                _branch = ExecuteCommand("git.exe", "describe --all")[0];
            }
            return _branch;
        }

        public virtual string GetTags()
        {
            if (_tags == null)
            {
                _tags = ExecuteCommand("git.exe", "describe")[0];
            }
            return _tags;
        }
    }
}
