using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Angelo
{
    public class Session
    {
        public HttpContext Context { get; set; }
    }
}