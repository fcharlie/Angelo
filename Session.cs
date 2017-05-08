using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Angelo
{
    public class Session
    {
        public HttpContext Context { get; set; }
        public async Task<bool> Processing(){
            await Context.Response.WriteAsync("hello world");
            return true;
        }
    }
}