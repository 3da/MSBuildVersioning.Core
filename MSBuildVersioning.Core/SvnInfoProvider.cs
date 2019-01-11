using System;
using System.Collections.Generic;

namespace MSBuildVersioning.Core
{
    /// <summary>
    /// Provides Subversion information for a particular file path, by executing and scraping
    /// information from the svn.exe command-line program.
    /// </summary>
    public class SvnInfoProvider : SourceControlInfoProvider
    {
        private int? _revisionNumber;
        private bool? _isMixedRevisions;
        private bool? _isWorkingCopyDirty;
        private string _repositoryUrl;
        private string _repositoryRoot;

        public override string SourceControlName
        {
            get { return "Subversion"; }
        }

        public virtual int GetRevisionNumber()
        {
            if (_revisionNumber == null)
            {
                SvnInfoParser parser = new SvnInfoParser();
                ExecuteCommand("svn.exe", "info -R", parser.ReadLine, null);
                _revisionNumber = parser.MaxRevisionNumber;
                _isMixedRevisions = parser.IsMixedRevisions;
            }
            return (int)_revisionNumber;
        }

        public virtual bool IsMixedRevisions()
        {
            if (_isMixedRevisions == null)
            {
                GetRevisionNumber();
            }
            return (bool)_isMixedRevisions;
        }

        public virtual bool IsWorkingCopyDirty()
        {
            if (_isWorkingCopyDirty == null)
            {
                SvnStatusParser parser = new SvnStatusParser();
                ExecuteCommand("svn.exe", "status", parser.ReadLine, null);
                _isWorkingCopyDirty = parser.IsWorkingCopyDirty;
            }
            return (bool)_isWorkingCopyDirty;
        }

        public virtual string GetRepositoryUrl()
        {
            if (_repositoryUrl == null)
            {
                IList<string> svnInfo = ExecuteCommand("svn.exe", "info");
                foreach (string line in svnInfo)
                {
                    if (line.StartsWith("URL: "))
                    {
                        _repositoryUrl = line.Substring("URL: ".Length);
                    }
                    else if (line.StartsWith("Repository Root: "))
                    {
                        _repositoryRoot = line.Substring("Repository Root: ".Length);
                    }
                }
            }
            return _repositoryUrl;
        }

        public virtual string GetRepositoryRoot()
        {
            if (_repositoryRoot == null)
            {
                GetRepositoryUrl();
            }
            return _repositoryRoot;
        }

        public virtual string GetRepositoryPath()
        {
            string path = GetRepositoryUrl().Substring(GetRepositoryRoot().Length);
            if (path.Length == 0)
            {
                return "/";
            }
            else
            {
                return path;
            }
        }

        public virtual string GetRepositorySubDirectory(string directory)
        {
            string[] pathComponents = GetRepositoryPath().Split('/');
            for (int i = 0; i < pathComponents.Length - 1; i++)
            {
                if (pathComponents[i] == directory)
                {
                    return pathComponents[i + 1];
                }
            }
            return "";
        }

        public virtual string GetBranch()
        {
            return GetRepositorySubDirectory("branches");
        }

        public virtual string GetTag()
        {
            return GetRepositorySubDirectory("tags");
        }

        private class SvnInfoParser
        {
            public int MaxRevisionNumber = -1;
            public bool IsMixedRevisions = false;

            public void ReadLine(string line)
            {
                if (line.StartsWith("Revision: "))
                {
                    int revision = int.Parse(line.Substring("Revision: ".Length));
                    if (MaxRevisionNumber >= 0 && MaxRevisionNumber != revision)
                    {
                        IsMixedRevisions = true;
                    }
                    MaxRevisionNumber = Math.Max(revision, MaxRevisionNumber);
                }
            }
        }

        private class SvnStatusParser
        {
            public bool IsWorkingCopyDirty = false;

            public void ReadLine(string line)
            {
                IsWorkingCopyDirty = true;
            }
        }
    }
}
