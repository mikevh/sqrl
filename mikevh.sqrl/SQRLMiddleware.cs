using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using mikevh.sqrl.Repos;

namespace mikevh.sqrl
{
    public class SQRLOptions
    {
        public Func<HttpContext,string,string> CPSPath { get; set; }
        public Action<string,int> Answer { get; set; }
    }

    public class CachedNut
    {
        public string idk { get; set; }
        public bool Authenticated { get; set; }
        public Guid Token { get; set; }
    }

    public class SQRLMiddleware
    {
        private readonly RequestDelegate _next;

        public SQRLMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            var options = ctx.RequestServices.GetService<SQRLOptions>() ?? new SQRLOptions();

            if (ctx.Request.Path != "/sqrl/auth" || ctx.Request.Method != HttpMethods.Post || !ctx.Request.IsHttps)
            {
                await _next(ctx);
                return;
            }

            //todo: validate nut
            var cache = ctx.RequestServices.GetService<IMemoryCache>();
            var userRepo = ctx.RequestServices.GetService<IUserRepo>();

            var auth = new SQRLVM
            {
                Client = ctx.Request.Form["client"],
                Ids = ctx.Request.Form["ids"],
                Server = ctx.Request.Form["server"]
            };
            var reqNut = ctx.Request.Query["nut"];

            var req = SQRL.DecodeRequest(ctx.Request.Host.Value, RequestIP(), auth);
            var user = userRepo.Get(req.idk);
            var res = SQRL.ComoseResponse(req, user, userRepo.Update, userRepo.Add, nut =>
            {
                cache.Set("CPS" + nut, user);
            }, nut =>
            {
                cache.Set("nonCPS" + nut, user);
            });

            if(req.opt.Contains("suk"))
            {
                res.suk = user.suk;
            }

            // scenarios when to link nuts
            // 1. query w/o cps
            //      
            // 2. ??

            // link req nut to resp nut for possible issuing to token bps
            // keyed by original nut, value is class
            // 
            // check for existing previous Server.nut?
            if(!req.opt.Contains("cps") 
               && req.cmd == "ident"
               && cache.TryGetValue(req.Server.nut, out CachedNut linked) 
               && linked.idk == req.idk)
            {
                linked.Authenticated = true;
                linked.Token = Guid.NewGuid();
            }
            if(!req.opt.Contains("cps") && req.cmd == "query")
            {
                cache.Set(reqNut, new CachedNut { idk = req.idk });
            }
            if(req.cmd != "query" && req.opt.Contains("cps"))
            {
                res.url = options.CPSPath(ctx, res.nut);
            }

            var rv = res.Serialize();
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = "application/x-www-form-urlencoded";
            ctx.Response.ContentLength = rv.Length;
            await ctx.Response.WriteAsync(rv);

            string RequestIP() => ctx.Request.IsHttps ? ctx.Request.Host.Host == "localhost" ? "127.0.0.1" : ctx.Request.Host.Host : "0.0.0.0";
        }
    }

    public static class SQRLExtension
    {
        public static void AddSQRL(this IServiceCollection services, Action<SQRLOptions> options)
        {
            var sqrlOptions = new SQRLOptions();
            options?.Invoke(sqrlOptions);

            services.AddSingleton(sqrlOptions);
            //services.AddSingleton<ISQRLCache, SQRLCache>();
            services.AddMemoryCache();
        }

        public static IApplicationBuilder UseSQRL(this IApplicationBuilder app)
        {
            // grab services here to call for logins, answers

            return app.UseMiddleware<SQRLMiddleware>();
        }

        public static string ToHex(this SQRLReponse.TIF tif) => tif.ToString("X").TrimStart('0');
    }
}
