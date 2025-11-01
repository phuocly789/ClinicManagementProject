// Services/IApiService.cs
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
public interface IApiService
{
    // Lấy dữ liệu (GET)
    Task<T?> GetAsync<T>(string requestUri);

    // Gửi dữ liệu (POST) - Thêm nếu cần
    Task<TResponse?> PostAsync<TRequest, TResponse>(string requestUri, TRequest data);

    // Cập nhật dữ liệu (PUT) - Thêm nếu cần
    // Task<TResponse?> PutAsync<TRequest, TResponse>(string requestUri, TRequest data);

    // Xóa dữ liệu (DELETE) - Thêm nếu cần
    // Task<bool> DeleteAsync(string requestUri);
}

public class ApiService : IApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(
        IHttpClientFactory httpClientFactory,
        AuthenticationStateProvider authenticationStateProvider
    )
    {
        _httpClientFactory = httpClientFactory;
        _authenticationStateProvider = authenticationStateProvider;

        // Cấu hình Json: không phân biệt hoa/thường, xử lý Enum dạng string
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            // Đảm bảo bạn đã thêm JsonStringEnumConverter trong Program.cs
            // Nếu API trả về Enum dạng số, XÓA converter này và sửa Enum trong DTO
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
        };
    }

    // --- Triển khai phương thức GET ---
    public async Task<T?> GetAsync<T>(string requestUri)
    {
        return await SendRequestInternalAsync<T>(HttpMethod.Get, requestUri);
    }

    // --- (Ví dụ) Triển khai phương thức POST ---
    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string requestUri, TRequest data)
    {
        // Chuyển đối tượng data thành JSON content
        using var jsonContent = JsonContent.Create(data, options: _jsonOptions);
        return await SendRequestInternalAsync<TResponse>(HttpMethod.Post, requestUri, jsonContent);
    }

    // --- Hàm Gửi Request Chung (nơi xử lý token và lỗi) ---
    private async Task<T?> SendRequestInternalAsync<T>(
        HttpMethod method,
        string requestUri,
        HttpContent? content = null
    )
    {
        // 1. Lấy HttpClient từ Factory (dùng client tên "ClinicManagement_API")
        var httpClient = _httpClientFactory.CreateClient("ClinicManagement_API");

        // 2. Lấy token MỚI NHẤT từ Provider
        //    Ép kiểu về provider cụ thể của bạn để gọi hàm lấy token
        var tokenProvider = (JwtAuthStateProvider)_authenticationStateProvider;
        string? token = null;
        try
        {
            token = await tokenProvider.GetTokenAsync(); // Dùng hàm gốc để đọc từ storage
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ApiService ERROR] Failed to get token: {ex.Message}");
            // Không có token, không thể tiếp tục gọi API cần xác thực
            return default; // Trả về null (hoặc throw lỗi)
        }

        Debug.WriteLine(
            $"[ApiService DEBUG] Token for {requestUri}: {(!string.IsNullOrEmpty(token) ? "PRESENT" : "MISSING/ERROR")}"
        );

        // 3. Tạo Request Message
        using var request = new HttpRequestMessage(method, requestUri);

        // 4. Gắn Header nếu có token
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            // Nếu API này yêu cầu token mà không có, không nên gửi request
            Debug.WriteLine(
                $"[ApiService WARNING] No token available for potentially authenticated request: {requestUri}"
            );
            // Quyết định: Trả về null hay throw lỗi? Tạm thời trả về null.
            return default;
        }

        // 5. Thêm Content nếu là POST/PUT
        if (content != null)
        {
            request.Content = content;
        }

        // 6. Gửi Request và xử lý Response
        HttpResponseMessage? httpResponse = null;
        try
        {
            Debug.WriteLine(
                $"[ApiService DEBUG] Sending {method} request to {httpClient.BaseAddress}{requestUri}"
            );
            httpResponse = await httpClient.SendAsync(request);

            Debug.WriteLine(
                $"[ApiService DEBUG] Received HTTP {(int)httpResponse.StatusCode} ({httpResponse.ReasonPhrase}) for {requestUri}"
            );

            if (httpResponse.IsSuccessStatusCode) // Chỉ kiểm tra mã 2xx
            {
                // Đọc và Deserialize nội dung
                try
                {
                    using var contentStream = await httpResponse.Content.ReadAsStreamAsync();
                    // Kiểm tra nếu stream trống (ví dụ cho 204 No Content)
                    if (contentStream.Length == 0)
                    {
                        Debug.WriteLine(
                            $"[ApiService DEBUG] Received empty successful response (e.g., 204 No Content) for {requestUri}"
                        );
                        // Nếu kiểu T là nullable (ví dụ Task<MyClass?>), trả về null là hợp lệ
                        // Nếu kiểu T không nullable (ví dụ Task<bool>), cần xử lý khác hoặc đảm bảo API không trả về 204 cho kiểu này
                        if (default(T) == null)
                            return default;
                        // Nếu T không phải nullable, có thể throw lỗi hoặc trả về giá trị mặc định hợp lệ khác
                        Debug.WriteLine(
                            $"[ApiService WARNING] Received empty response for non-nullable type {typeof(T).Name}. Returning default."
                        );
                        return default; // Hoặc default(T) nếu T là struct
                    }

                    // Deserialize từ stream
                    var result = await JsonSerializer.DeserializeAsync<T>(
                        contentStream,
                        _jsonOptions
                    );
                    Debug.WriteLine(
                        $"[ApiService DEBUG] Successfully deserialized response for {requestUri}"
                    );
                    return result;
                }
                catch (JsonException jsonEx)
                {
                    Debug.WriteLine(
                        $"[ApiService ERROR] JSON Parse Error for {requestUri}: {jsonEx.Message}"
                    );
                    string rawContent = await httpResponse.Content.ReadAsStringAsync(); // Đọc nội dung thô để debug
                    Debug.WriteLine(
                        $"[ApiService RAW CONTENT on JSON Error]: {rawContent.Substring(0, Math.Min(rawContent.Length, 500))}..."
                    ); // Giới hạn độ dài log
                    return default;
                }
            }
            else
            {
                // Xử lý lỗi HTTP (4xx, 5xx)
                string errorContent = await httpResponse.Content.ReadAsStringAsync(); // Đọc nội dung lỗi nếu có
                Debug.WriteLine(
                    $"[ApiService ERROR] HTTP Error {(int)httpResponse.StatusCode} ({httpResponse.ReasonPhrase}) for {requestUri}. Content: {errorContent.Substring(0, Math.Min(errorContent.Length, 500))}..."
                );

                // Đặc biệt xử lý 401 (Unauthorized)
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Debug.WriteLine(
                        "[ApiService ERROR] Unauthorized! Token might be invalid or expired. Consider redirecting to login."
                    );
                    // TODO: Implement logic to handle unauthorized access (e.g., redirect to login)
                    // Ví dụ: NavigationManager.NavigateTo("/logout", forceLoad: true); (Cần inject NavigationManager)
                }
                // Xử lý 403 (Forbidden) - Biết user là ai nhưng không có quyền
                else if (httpResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    Debug.WriteLine(
                        "[ApiService ERROR] Forbidden! User does not have permission for this resource."
                    );
                    // Hiển thị thông báo lỗi phù hợp
                }

                // Với các lỗi khác, chỉ log và trả về null
                return default;
            }
        }
        catch (HttpRequestException httpEx) // Lỗi mạng (không kết nối được server, DNS...)
        {
            Debug.WriteLine($"[ApiService NETWORK ERROR] for {requestUri}: {httpEx.Message}");
            return default;
        }
        catch (TaskCanceledException cancelEx) // Timeout hoặc request bị hủy
        {
            Debug.WriteLine($"[ApiService TIMEOUT/CANCELLED] for {requestUri}: {cancelEx.Message}");
            return default;
        }
        catch (Exception ex) // Các lỗi không mong muốn khác
        {
            Debug.WriteLine(
                $"[ApiService UNEXPECTED ERROR] for {requestUri}: {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}"
            );
            return default;
        }
        finally
        {
            // Giải phóng response message nếu nó được tạo
            httpResponse?.Dispose();
        }
    }
}
