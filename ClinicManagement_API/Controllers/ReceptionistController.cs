using ClinicManagementSystem.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/receptionist")]
// Áp dụng cho toàn bộ Controller: Chỉ có vai trò "Receptionist" mới được truy cập
[Authorize(Roles = RoleNameConst.Receptionist)]
public class ReceptionistController : ControllerBase
{
    private readonly IReceptionistService _receptionistService;

    public ReceptionistController(IReceptionistService receptionistService)
    {
        _receptionistService = receptionistService;
    }

    /// Lễ tân tạo một hồ sơ bệnh nhân và tài khoản mới.
    [HttpPost("CreatePatient")]
    // Kế thừa [Authorize] từ class, chỉ Lễ tân được dùng
    public async Task<IActionResult> CreatePatient(
        [FromBody] CreatePatientByReceptionistDto patientDto
    )
    {
        var response = await _receptionistService.CreatePatientAndAccountAsync(patientDto);

        return response.Status switch
        {
            StatusReponse.Success => Ok(response.Content),
            StatusReponse.BadRequest => BadRequest(new { message = response.Message }),
            _ => StatusCode(
                500,
                new { message = response.Message ?? "Đã xảy ra lỗi không xác định." }
            ),
        };
    }

    /// Lễ tân reset mật khẩu cho một bệnh nhân.
    [HttpPost("patients/{patientId}/reset-password")]
    // Kế thừa [Authorize] từ class, chỉ Lễ tân được dùng
    public async Task<IActionResult> ResetPatientPassword(int patientId)
    {
        var result = await _receptionistService.ResetPasswordAsync(patientId);

        if (!result)
        {
            return NotFound(new { message = "Không tìm thấy bệnh nhân với ID cung cấp." });
        }

        return Ok(
            new
            {
                message = "Reset mật khẩu thành công. Mật khẩu mặc định mới là số điện thoại của bệnh nhân.",
            }
        );
    }

    // VÍ DỤ VỀ VIỆC CHO PHÉP NHIỀU VAI TRÒ
    /// <summary>
    /// Lấy thông tin chi tiết của một bệnh nhân.
    /// </summary>
    /// <remarks>
    /// Action này có thể được truy cập bởi Lễ tân, Bác sĩ, và Admin.
    /// </remarks>
    [HttpGet("patients/{patientId}")]
    [Authorize(
        Roles = $"{RoleNameConst.Receptionist},{RoleNameConst.Doctor},{RoleNameConst.Admin}"
    )]
    public async Task<IActionResult> GetPatientDetails(int patientId)
    {
        // Code ví dụ
        return Ok(
            $"Đang lấy thông tin cho bệnh nhân có ID = {patientId}. Chỉ Lễ tân, Bác sĩ, Admin mới thấy được thông báo này."
        );
    }
}
