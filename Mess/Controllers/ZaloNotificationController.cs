using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MESS.Application.DTOs.Zalo;
using MESS.Application.Interfaces.Zalo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MESS.Mess.Controllers;

[ApiController]
[Route("api/notifications/zalo")]
public class ZaloNotificationController : ControllerBase
{
    private readonly IZaloNotificationService _zaloService;

    public ZaloNotificationController(IZaloNotificationService zaloService)
    {
        _zaloService = zaloService;
    }

    /// <summary>
    /// Lấy danh sách khách hàng / học viên có thể nhận thông báo Zalo
    /// </summary>
    [HttpGet("recipients")]
    public async Task<IActionResult> GetRecipients()
    {
        var recipients = await _zaloService.GetRecipientsAsync();
        return Ok(new { success = true, data = recipients });
    }

    /// <summary>
    /// Đẩy thông báo ca học / lịch học qua Zalo OA (ZNS Simulator) cho danh sách khách hàng
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendScheduleNotification([FromBody] SendZaloScheduleRequest request)
    {
        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentUserName = User.FindFirstValue(ClaimTypes.Name) ?? "Nhân viên Quản lý";
        
        Guid senderId = Guid.TryParse(currentUserIdStr, out var id) ? id : Guid.Empty;

        var result = await _zaloService.SendScheduleNotificationBatchAsync(request, senderId, currentUserName);
        return Ok(result);
    }

    /// <summary>
    /// Lấy lịch sử tất cả các thông báo Zalo đã gửi
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] int limit = 50)
    {
        var history = await _zaloService.GetHistoryAsync(limit);
        return Ok(new { success = true, data = history });
    }

    /// <summary>
    /// Lấy hộp thư Zalo mô phỏng của 1 khách hàng cụ thể
    /// </summary>
    [HttpGet("customer/{customerId:guid}/inbox")]
    public async Task<IActionResult> GetCustomerInbox([FromRoute] Guid customerId)
    {
        var inbox = await _zaloService.GetCustomerInboxAsync(customerId);
        return Ok(new { success = true, data = inbox });
    }
}
