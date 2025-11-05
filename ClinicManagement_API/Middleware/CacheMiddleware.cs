using System.Text;
using Microsoft.Extensions.Caching.Memory;

public class CacheMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;

    public CacheMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Cache chỉ GET request thôi
        if (context.Request.Method != HttpMethods.Get)
        {
            await _next(context);
            return;
        }

        var key = context.Request.Path + context.Request.QueryString;

        if (_cache.TryGetValue(key, out string cachedResponse))
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(cachedResponse);
            return;
        }

        // Lưu body response vào Memory
        var originalBodyStream = context.Response.Body;
        using var newBodyStream = new MemoryStream();
        context.Response.Body = newBodyStream;

        await _next(context); // chạy tiếp xuống API

        newBodyStream.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(newBodyStream).ReadToEndAsync();

        _cache.Set(key, responseText, TimeSpan.FromSeconds(30)); // ⏱️ Cache 30 giây

        newBodyStream.Seek(0, SeekOrigin.Begin);
        await newBodyStream.CopyToAsync(originalBodyStream);
    }
}
