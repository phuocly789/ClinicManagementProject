using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;
    private readonly JwtSecurityTokenHandler _handler = new();
    private const string TOKEN_KEY = "auth.token";

    public JwtAuthStateProvider(IJSRuntime js)
    {
        _js = js;
    }

    public async Task SetTokenAsync(string? token)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", TOKEN_KEY, token ?? "");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task<string?> GetTokenAsync()
    {
        var token = await _js.InvokeAsync<string>("localStorage.getItem", TOKEN_KEY);
        return string.IsNullOrWhiteSpace(token) ? null : token;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            var jwt = _handler.ReadJwtToken(token);

            var exp = jwt.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            if (
                exp is not null
                && DateTimeOffset.UtcNow >= DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp))
            )
            {
                await SetTokenAsync(null);
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var identity = new ClaimsIdentity(jwt.Claims, "jwt", ClaimTypes.Name, "role");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        // Bắt lỗi khi prerendering
        catch (InvalidOperationException)
        {
            // Trong quá trình prerendering, không thể truy cập JS.
            // Trả về trạng thái chưa xác thực.
            // Blazor sẽ tự động gọi lại phương thức này sau khi kết nối SignalR được thiết lập.
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }
}
