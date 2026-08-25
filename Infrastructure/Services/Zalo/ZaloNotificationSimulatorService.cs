using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MESS.Application.DTOs.Zalo;
using MESS.Application.Interfaces.Zalo;
using MESS.Domain.Entities;
using MESS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MESS.Infrastructure.Services.Zalo;

public class ZaloNotificationSimulatorService : IZaloNotificationService
{
    private readonly MessDbContext _context;
    private readonly IConfiguration _config;
    private readonly ILogger<ZaloNotificationSimulatorService> _logger;

    public ZaloNotificationSimulatorService(
        MessDbContext context,
        IConfiguration config,
        ILogger<ZaloNotificationSimulatorService> logger)
    {
        _context = context;
        _config = config;
        _logger = logger;
    }

    public async Task<ZaloSendBatchResponse> SendScheduleNotificationBatchAsync(
        SendZaloScheduleRequest request,
        Guid senderUserId,
        string senderName)
    {
        var response = new ZaloSendBatchResponse();

        if (request.CustomerIds == null || request.CustomerIds.Count == 0)
        {
            response.Success = false;
            response.Message = "Vui lòng chọn ít nhất một khách hàng để gửi thông báo Zalo.";
            return response;
        }

        // Lấy danh sách thông tin khách hàng từ DB
        var customers = await _context.Users
            .Include(u => u.Department)
            .Where(u => request.CustomerIds.Contains(u.Id))
            .ToListAsync();

        if (customers.Count == 0)
        {
            response.Success = false;
            response.Message = "Không tìm thấy khách hàng nào hợp lệ.";
            return response;
        }

        var results = new List<ZaloNotificationLogResponse>();
        var logsToSave = new List<ZaloNotificationLog>();

        _logger.LogInformation("Bắt đầu mô phỏng gửi Zalo OA/ZNS cho {Count} khách hàng bởi {Sender}", customers.Count, senderName);

        foreach (var cust in customers)
        {
            // Giả lập số điện thoại nếu user dùng email
            var phone = cust.Username.Contains("@") 
                ? "09" + Math.Abs(cust.Id.GetHashCode() % 100000000).ToString("D8") 
                : cust.Username;

            var msgId = $"ZNS-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

            var log = new ZaloNotificationLog
            {
                Id = Guid.NewGuid(),
                CustomerId = cust.Id,
                CustomerName = cust.FullName,
                CustomerPhone = phone,
                CourseName = string.IsNullOrWhiteSpace(request.CourseName) ? "Khóa học Kỹ năng Quản trị MES" : request.CourseName,
                ShiftTime = string.IsNullOrWhiteSpace(request.ShiftTime) ? "08:30 - 11:30 Hôm nay" : request.ShiftTime,
                RoomUrl = string.IsNullOrWhiteSpace(request.RoomUrl) ? "https://zoom.us/j/9821234891" : request.RoomUrl,
                TeacherName = string.IsNullOrWhiteSpace(request.TeacherName) ? senderName : request.TeacherName,
                CustomNote = request.CustomNote ?? string.Empty,
                SentByUserId = senderUserId,
                SentByUserName = senderName,
                Status = "SUCCESS",
                Channel = "ZALO_ZNS_SIMULATOR",
                MessageId = msgId,
                SentAt = DateTime.UtcNow
            };

            logsToSave.Add(log);

            results.Add(new ZaloNotificationLogResponse
            {
                Id = log.Id,
                CustomerId = log.CustomerId,
                CustomerName = log.CustomerName,
                CustomerPhone = log.CustomerPhone,
                CourseName = log.CourseName,
                ShiftTime = log.ShiftTime,
                RoomUrl = log.RoomUrl,
                TeacherName = log.TeacherName,
                CustomNote = log.CustomNote,
                SentByUserId = log.SentByUserId,
                SentByUserName = log.SentByUserName,
                Status = log.Status,
                Channel = log.Channel,
                MessageId = log.MessageId,
                SentAt = log.SentAt
            });
        }

        await _context.ZaloNotificationLogs.AddRangeAsync(logsToSave);
        await _context.SaveChangesAsync();

        // Giả lập độ trễ mạng API Zalo (200ms)
        await System.Threading.Tasks.Task.Delay(200);

        response.Success = true;
        response.TotalSent = customers.Count;
        response.SuccessCount = customers.Count;
        response.FailCount = 0;
        response.Results = results;
        response.Message = $"Đã đẩy thông báo Zalo OA thành công tới {customers.Count} khách hàng!";

        _logger.LogInformation("Hoàn tất gửi Zalo OA Simulator cho {Count} khách hàng.", customers.Count);
        return response;
    }

    public async Task<List<ZaloNotificationLogResponse>> GetHistoryAsync(int limit = 50)
    {
        return await _context.ZaloNotificationLogs
            .OrderByDescending(l => l.SentAt)
            .Take(limit)
            .Select(l => new ZaloNotificationLogResponse
            {
                Id = l.Id,
                CustomerId = l.CustomerId,
                CustomerName = l.CustomerName,
                CustomerPhone = l.CustomerPhone,
                CourseName = l.CourseName,
                ShiftTime = l.ShiftTime,
                RoomUrl = l.RoomUrl,
                TeacherName = l.TeacherName,
                CustomNote = l.CustomNote,
                SentByUserId = l.SentByUserId,
                SentByUserName = l.SentByUserName,
                Status = l.Status,
                Channel = l.Channel,
                MessageId = l.MessageId,
                SentAt = l.SentAt
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<ZaloNotificationLogResponse>> GetCustomerInboxAsync(Guid customerId)
    {
        return await _context.ZaloNotificationLogs
            .Where(l => l.CustomerId == customerId)
            .OrderByDescending(l => l.SentAt)
            .Select(l => new ZaloNotificationLogResponse
            {
                Id = l.Id,
                CustomerId = l.CustomerId,
                CustomerName = l.CustomerName,
                CustomerPhone = l.CustomerPhone,
                CourseName = l.CourseName,
                ShiftTime = l.ShiftTime,
                RoomUrl = l.RoomUrl,
                TeacherName = l.TeacherName,
                CustomNote = l.CustomNote,
                SentByUserId = l.SentByUserId,
                SentByUserName = l.SentByUserName,
                Status = l.Status,
                Channel = l.Channel,
                MessageId = l.MessageId,
                SentAt = l.SentAt
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<CustomerRecipientItem>> GetRecipientsAsync()
    {
        var users = await _context.Users
            .Include(u => u.Department)
            .Where(u => u.IsActive)
            .OrderBy(u => u.FullName)
            .AsNoTracking()
            .ToListAsync();

        var recentLogs = await _context.ZaloNotificationLogs
            .GroupBy(l => l.CustomerId)
            .Select(g => new
            {
                CustomerId = g.Key,
                Count = g.Count(),
                LastDate = g.Max(x => x.SentAt)
            })
            .ToDictionaryAsync(x => x.CustomerId, x => x);

        return users.Select(u =>
        {
            var phone = u.Username.Contains("@")
                ? "09" + Math.Abs(u.Id.GetHashCode() % 100000000).ToString("D8")
                : u.Username;

            recentLogs.TryGetValue(u.Id, out var logInfo);

            return new CustomerRecipientItem
            {
                Id = u.Id,
                FullName = u.FullName,
                Username = u.Username,
                PhoneNumber = phone,
                DepartmentName = u.Department?.Name ?? "Chưa phân bổ",
                PositionTitle = u.PositionTitle ?? "Học viên / Nhân sự",
                TotalNotificationsReceived = logInfo?.Count ?? 0,
                LastNotifiedAt = logInfo?.LastDate
            };
        }).ToList();
    }
}
