using System;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Angelo
{
    public class Session
    {
        private HttpContext Context { get; }
        public Session(HttpContext context) 
        {
            this.Context = context;
               
        }
        private async Task Unauthorized()
        {
            Context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            Context.Response.Headers.Add("WWW-Authenticate", $"Basic realm=\"{AppconfigProvider.config.Realm}\"");
            await Context.Response.WriteAsync("Unauthorized");
        }
        private async Task NotFound()
        {
            Context.Response.StatusCode = StatusCodes.Status404NotFound;
            await Context.Response.WriteAsync("Not Found");
        }

        void HeadersFill()
        {
            Context.Response.Headers["Expires"] = "Fri, 01 Jan 1980 00:00:00 GMT";
            Context.Response.Headers["Pragma"] = "no-cache";
            Context.Response.Headers["Cache-Control"] = "no-cache, max-age=0, must-revalidate";
        }

        private async Task ExchangeRefs()
        {
            var url = Context.Request.Path.ToString();
            if (!url.EndsWith("/info/refs") || !Context.Request.Query.ContainsKey("service"))
            {
                Context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await Context.Response.WriteAsync("Bad Request !");
                return;
            }
            var path = url.Substring(0, url.Length - "/info/refs".Length);
            string authtxt = null;
            if (Context.Request.Headers.ContainsKey("Authorization"))
            {
                authtxt = Context.Request.Headers["Authorization"].ToString();
            }
            var result = AppconfigProvider.Authorize(authtxt, path);
            if (result == null)
            {
                await Unauthorized();
                return;
            }
            var repodir = AppconfigProvider.PathcombineRoot(path);
            if (!Directory.Exists(repodir))
            {
                await NotFound();
                return;
            }
            HeadersFill();
            Process process = new Process();
            process.StartInfo.FileName = AppconfigProvider.config.Gitbin;
            process.StartInfo.Environment.Add("GL_ID", $"user-{result.Userid}");
            process.StartInfo.RedirectStandardOutput = true;
            switch (Context.Request.Query["service"])
            {
                case "git-upload-pack":
                    {
                        process.StartInfo.Arguments = $"upload-pack --stateless-rpc --advertise-refs \"{repodir}\"";
                        Context.Response.ContentType = "application/x-git-upload-pack-advertisement";
                        var bytes = System.Text.Encoding.UTF8.GetBytes("001e# service=git-upload-pack\n0000");
                        await Context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                    }
                    break;
                case "git-receive-pack":
                    {
                        process.StartInfo.Arguments = $"receive-pack --stateless-rpc --advertise-refs \"{repodir}\"";
                        Context.Response.ContentType = "application/x-git-receive-pack-advertisement";
                        var bytes2 = System.Text.Encoding.UTF8.GetBytes("001e# service=git-upload-pack\n0000");
                        await Context.Response.Body.WriteAsync(bytes2, 0, bytes2.Length);
                    }
                    break;
                default:
                    {
                        Context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await Context.Response.WriteAsync("Invalid service !");
                    }
                    break;
            }
            if (!process.Start())
            {
                Context.Response.StatusCode = StatusCodes.Status404NotFound;
                await Context.Response.WriteAsync("Git Not Found");
            }
            await process.StandardOutput.BaseStream.CopyToAsync(Context.Response.Body);
        }
        private async Task ExchangePackets()
        {
            var url = Context.Request.Path.ToString();
            var index = url.LastIndexOf('/');
            if (index == -1)
            {
                Context.Response.StatusCode = StatusCodes.Status404NotFound;
                await Context.Response.WriteAsync("Not Found!");
                return;
            }
            var service = url.Substring(index + 1);
            var path = url.Substring(0, index);
            string authtxt = null;
            if (Context.Request.Headers.ContainsKey("Authorization"))
            {
                authtxt = Context.Request.Headers["Authorization"].ToString();
            }
            var result = AppconfigProvider.Authorize(authtxt, path);
            if (result == null)
            {
                await Unauthorized();
                return;
            }
            var repodir = AppconfigProvider.PathcombineRoot(path);
            if (!Directory.Exists(repodir))
            {
                await NotFound();
                return;
            }
            HeadersFill();
            Process process = new Process();
            process.StartInfo.FileName = AppconfigProvider.config.Gitbin;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.Environment.Add("GL_ID", $"user-{result.Userid}");
            if (service == "git-upload-pack")
            {
                process.StartInfo.Arguments = "upload-pack  --stateless-rpc  \"" + repodir + "\"";
                Context.Response.ContentType = "application/x-git-upload-pack-result";
            }
            else if (service == "git-receive-pack")
            {
                process.StartInfo.Arguments = "receive-pack  --stateless-rpc \"" + repodir + "\"";
                Context.Response.ContentType = "application/x-git-receive-pack-result";

            }
            else
            {
                Context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await Context.Response.WriteAsync("Invalid service !");
                return;
            }
            if (!process.Start())
            {
                Context.Response.StatusCode = StatusCodes.Status404NotFound;
                await Context.Response.WriteAsync("Git Not Found");
            }
            if (Context.Request.Headers.ContainsKey("Content-Encoding") && Context.Request.Headers["Content-Encoding"].Equals("gzip"))
            {
                var input = new GZipStream(Context.Request.Body, CompressionMode.Decompress);
                await input.CopyToAsync(process.StandardInput.BaseStream);
            }
            else
            {
                await Context.Request.Body.CopyToAsync(process.StandardInput.BaseStream);
                await process.StandardInput.WriteAsync('\0');
            }
            process.StandardInput.Dispose();
            await process.StandardOutput.BaseStream.CopyToAsync(Context.Response.Body);
        }
        public async Task Handle()
        {
            Context.Response.Headers["Server"] = "Angelo/1.0";
            if (Context.Request.Method == "GET")
            {
                await ExchangeRefs();
            }
            else if (Context.Request.Method == "POST")
            {
                await ExchangePackets();
            }
            else
            {
                Context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                await Context.Response.WriteAsync("Method Not Allowed");
            }
            //Context.Request.Path;
        }
    }
}