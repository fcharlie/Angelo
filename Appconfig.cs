using System;
using System.Text;
using System.Linq;

namespace Angelo
{
    public class AuthResult
    {
        public int Result { get; set; }
        public long UID { get; set; }
    }
    public class Appconfig
    {
        public string Root { get; set; }
        public string AuthorizeURL { get; set; }
        public string Realm { get; set; }
        public string Gitbin { get; set; }
    }
    public class AppconfigProvider
    {
        public static Appconfig config { get; set; }
        public static string PathcombineRoot(string location)
        {
            var sb = new StringBuilder(config.Root);
            if (location.First() != '/')
            {
                sb.Append('/');
            }
            sb.Append(location);
            if (!location.EndsWith(".git"))
            {
                sb.Append(".git");
            }
            return System.IO.Path.GetFullPath(sb.ToString());
        }
        public static AuthResult Authorize(string authtext, string pwn)
        {
            /// NOT IMPL
            var result = new AuthResult
            {
                Result = 0,
                UID = 1
            };
            return result;
        }
    }
}