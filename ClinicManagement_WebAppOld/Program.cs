using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using Blazored.LocalStorage;
using ClinicManagement_WebApp;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

var builder = WebApplication.CreateBuilder(args);

// Blazor Server kiểu cũ (đơn giản, ổn định)
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddDataProtection();
builder.Services.AddAuthorizationCore();
builder.Services.AddSingleton<JwtSecurityTokenHandler>();
builder.Services.AddScoped<JwtAuthStateProvider>();


builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<JwtAuthStateProvider>()
);


builder.Services.AddTransient<BearerHandler>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddHttpClient(
    "ClinicManagement_API_NoAuth",
    c =>
    {
        c.BaseAddress = new Uri("http://localhost:5066/api/");
    }
);

builder
    .Services.AddHttpClient(
        "ClinicManagement_API",
        c =>
        {
            c.BaseAddress = new Uri("http://localhost:5066/api/");
        }
    )
    .AddHttpMessageHandler<BearerHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host"); // dùng _Host.cshtml

app.Run();

public class BearerHandler : DelegatingHandler
{
    private readonly JwtAuthStateProvider _auth;

    public BearerHandler(JwtAuthStateProvider auth) => _auth = auth;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage req,
        CancellationToken ct
    )
    {
        var token = await _auth.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return await base.SendAsync(req, ct);
    }
}
