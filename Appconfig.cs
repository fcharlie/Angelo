using System;
using System.Text;
using System.Linq;

namespace Angelo
{
    public class AuthResult
    {
        public int Result { get; set; }
        public int Userid { get; set; }
    }
    public class Appconfig
    {
        public string Root { get; set; }
        public bool PathConvert { get; set; } = false;
        public string AuthorizeURL { get; set; }
        public string Realm { get; set; }
        public string Gitbin { get; set; }
    }
    public class AppconfigProvider
    {
        public static Appconfig config { get; set; }
        public static string PathcombineRoot(string repodir)
        {
            var sb = new StringBuilder(config.Root);
            if (config.PathConvert)
            {
                if (repodir.Length < 3)
                    return null;
                sb.Append('/');
                if (repodir.First() == '/')
                {
                    sb.Append(repodir.ElementAt(1));
                    sb.Append(repodir.ElementAt(2));
                }
                else
                {
                    sb.Append(repodir.ElementAt(0));
                    sb.Append(repodir.ElementAt(1));
                    sb.Append('/');
                }
                sb.Append(repodir);
                return System.IO.Path.GetFullPath(sb.ToString());
            }
            if (repodir.First() != '/')
            {
                sb.Append('/');
            }
            sb.Append(repodir);
            return System.IO.Path.GetFullPath(sb.ToString());
        }
        public static AuthResult Authorize(string authtext, string pwn)
        {
            /// NOT IMPL
            var result = new AuthResult
            {
                Result = 0,
                Userid = 1
            };
            return result;
        }
    }
}
