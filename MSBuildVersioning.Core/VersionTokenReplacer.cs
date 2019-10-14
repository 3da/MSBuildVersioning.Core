using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace MSBuildVersioning.Core
{
    public delegate string TokenFunction();
    public delegate string TokenFunction<T>(T arg);
    public delegate string TokenFunction<T1, T2>(T1 arg1, T2 arg2);

    /// <summary>
    /// Replaces tokens in a string with basic project versioning information.
    /// </summary>
    public class VersionTokenReplacer
    {
        private readonly IList<Token> _tokens;

        public SourceControlInfoProvider SourceControlInfoProvider { get; set; }

        public VersionTokenReplacer()
        {
            _tokens = new List<Token>();

            AddToken("YEAR", () => DateTime.Now.ToString("yyyy"));
            AddToken("MONTH", () => DateTime.Now.ToString("MM"));
            AddToken("DAY", () => DateTime.Now.ToString("dd"));
            AddToken("DATE", () => DateTime.Now.ToString("yyyy-MM-dd"));
            AddToken("DATETIME", () => DateTime.Now.ToString("s"));

            AddToken("UTCYEAR", () => DateTime.UtcNow.ToString("yyyy"));
            AddToken("UTCMONTH", () => DateTime.UtcNow.ToString("MM"));
            AddToken("UTCDAY", () => DateTime.UtcNow.ToString("dd"));
            AddToken("UTCDATE", () => DateTime.UtcNow.ToString("yyyy-MM-dd"));
            AddToken("UTCDATETIME", () => DateTime.UtcNow.ToString("s"));

            AddToken("USER", () => Environment.UserName);
            AddToken("MACHINE", () => Environment.MachineName);
            AddToken("ENVIRONMENT", GetEnvironmentValue);
            AddToken("FILE", GetFileValue);
        }

        protected void AddToken(string tokenName, TokenFunction function)
        {
            _tokens.Add(new NoArgsToken
            {
                TokenName = tokenName,
                Function = function
            });
        }

        protected void AddToken(string tokenName, TokenFunction<int> function)
        {
            _tokens.Add(new IntArgToken
            {
                TokenName = tokenName,
                Function = function
            });
        }

        protected void AddToken(string tokenName, TokenFunction<string> function)
        {
            _tokens.Add(new StringArgToken
            {
                TokenName = tokenName,
                Function = function
            });
        }

        protected void AddToken(string tokenName, TokenFunction<string, string> function)
        {
            _tokens.Add(new TwoStringArgToken
            {
                TokenName = tokenName,
                Function = function
            });
        }

        public virtual string Replace(string content)
        {
            foreach (Token token in _tokens)
            {
                content = token.Replace(content);
            }
            return content;
        }

        private abstract class Token
        {
            public string TokenName;

            public abstract string Replace(string str);
        }

        private class NoArgsToken : Token
        {
            public TokenFunction Function;

            public override string Replace(string str)
            {
                string token = "$" + TokenName + "$";
                if (str.Contains(token))
                {
                    str = str.Replace(token, Function());
                }
                return str;
            }
        }

        private class IntArgToken : Token
        {
            public TokenFunction<int> Function;

            public override string Replace(string str)
            {
                MatchCollection revnumModMatches = Regex.Matches(str,
                    @"\$" + TokenName + @"\((\d+)\)\$");
                foreach (Match match in revnumModMatches)
                {
                    string token = match.Groups[0].Value;
                    int arg = int.Parse(match.Groups[1].Value);
                    str = str.Replace(token, Function(arg));
                }
                return str;
            }
        }

        private class StringArgToken : Token
        {
            public TokenFunction<string> Function;

            public override string Replace(string str)
            {
                MatchCollection revnumModMatches = Regex.Matches(str,
                    @"\$" + TokenName + @"\(""(.+?)""\)\$");
                foreach (Match match in revnumModMatches)
                {
                    string token = match.Groups[0].Value;
                    string arg = match.Groups[1].Value;
                    str = str.Replace(token, Function(arg));
                }
                return str;
            }
        }

        private class TwoStringArgToken : Token
        {
            public TokenFunction<string, string> Function;

            public override string Replace(string str)
            {
                MatchCollection revnumModMatches = Regex.Matches(str,
                    @"\$" + TokenName + @"\(""(.+?)"",""(.*?)""\)\$");
                foreach (Match match in revnumModMatches)
                {
                    string token = match.Groups[0].Value;
                    string arg1 = match.Groups[1].Value;
                    string arg2 = match.Groups[2].Value;
                    str = str.Replace(token, Function(arg1, arg2));
                }
                return str;
            }
        }

        private string GetEnvironmentValue(string name, string defaultValue)
        {
            var returnValue = Environment.GetEnvironmentVariable(name);
            return returnValue ?? defaultValue;
        }

        private string GetFileValue(string path)
        {
            if (!File.Exists(path))
                return string.Empty;
            return File.ReadLines(path).FirstOrDefault();
        }
    }
}
