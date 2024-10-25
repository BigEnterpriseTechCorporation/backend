using System.Diagnostics;

namespace Backend;

public class TimingMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        var watch = new Stopwatch();
        watch.Start();

        context.Response.OnStarting(state => {
            var httpContext = (HttpContext)state;
            httpContext.Response.Headers.Append("Response-Time", new[] { watch.ElapsedMilliseconds + " ms" });

            return Task.CompletedTask;
        }, context);

        await next(context);
    }
}

public static class TimingMiddlewareExtensions
{
    public static IApplicationBuilder UseTimingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TimingMiddleware>();
    }
}