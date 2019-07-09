using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using mikevh.sqrl.Repos;

namespace mikevh.sqrl
{
    public static class SQRLExtension
    {
        public static void AddSQRL(this IServiceCollection services, Action<SQRLOptions> options = null)
        {
            var sqrlOptions = new SQRLOptions();
            options?.Invoke(sqrlOptions);

            services.AddSingleton(sqrlOptions);
            services.AddMemoryCache();
        }

        public static IApplicationBuilder UseSQRL(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SQRLMiddleware>();
        }

        public static string ToHex(this SQRLReponse.TIF tif) => tif.ToString("X").TrimStart('0');
    }

    public class SQRLOptions
    {
        public string Path { get; set; } = "/sqrl/auth";
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

            if (ctx.Request.Path != options.Path || ctx.Request.Method != HttpMethods.Post || !ctx.Request.IsHttps)
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
            var nut = ctx.Request.Query["nut"];

            var req = SQRL.DecodeRequest(ctx.Request.Host.Value, RequestIP(), auth);
            var res = SQRL.ComoseResponse(req, userRepo.Get, userRepo.Update, (key, user) =>
            {
                cache.Set(key, user);
            });

            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = "application/x-www-form-urlencoded";
            var rv = res.Serialize();
            ctx.Response.ContentLength = rv.Length;
            await ctx.Response.WriteAsync(rv);

            string RequestIP() => ctx.Request.IsHttps? ctx.Request.Host.Host == "localhost" ? "127.0.0.1" : ctx.Request.Host.Host : "0.0.0.0";
        }
    }
}
