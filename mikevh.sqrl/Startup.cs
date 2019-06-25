using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace mikevh.sqrl
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseSQRL();
            app.UseStaticFiles();
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }

    public static class SqrlExtension
    {
        public static IApplicationBuilder UseSQRL(this IApplicationBuilder b) => UseSQRL(b, o => { });
        public static IApplicationBuilder UseSQRL(this IApplicationBuilder b, Action<SQRLOptions> options) => b.UseMiddleware<SQRLMiddleware>(options());

        private static readonly RouteData EmptyRouteData = new RouteData();

        private static readonly ActionDescriptor EmptyActionDescriptor = new ActionDescriptor();

        public static Task WriteResultAsync<T>(this HttpContext ctx, T result) where T : IActionResult
        { 
            if(ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx));
            }

            var ex = ctx.RequestServices.GetService<IActionResultExecutor<T>>();

            if(ex == null)
            {
                throw new InvalidOperationException($"No result executor for '{typeof(T).FullName}'");
            }

            var routeData = ctx.GetRouteData() ?? EmptyRouteData;
            var actionCtx = new ActionContext(ctx, routeData, EmptyActionDescriptor);
            return ex.ExecuteAsync(actionCtx, result);
        }
    }

    public class SQRLOptions
    {
        public string Path { get; set; } = "/sqrl/auth";
    }

    public class SQRLMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SQRLOptions _options;

        public SQRLMiddleware(RequestDelegate next, SQRLOptions options = null)
        {
            _options = options ?? new SQRLOptions();
            _next = next;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            if(ctx.Request.Path == _options.Path && ctx.Request.Method == HttpMethods.Post)
            {
                var r = new ContentResult
                {
                    Content = "foo",
                    ContentType = "text/plain", // todo: what should this be
                    StatusCode = StatusCodes.Status200OK
                };

                await ctx.WriteResultAsync(r);
            }

            await _next(ctx);
        }
    }
}
