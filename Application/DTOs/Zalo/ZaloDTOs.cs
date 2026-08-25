using System;
using System.Collections.Generic;

namespace MESS.Application.DTOs.Zalo;

public class SendZaloScheduleRequest
{
    public List<Guid> CustomerIds { get; set; } = new();
    public string CourseName { get; set; } = string.Empty;
    public string ShiftName { get; set; } = string.Empty;
    public string ShiftTime { get; set; } = string.Empty;
    public string RoomUrl { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string CustomNote { get; set; } = string.Empty;
    public string BannerUrl { get; set; } = string.Empty;
}

public class ZaloNotificationLogResponse
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string ShiftTime { get; set; } = string.Empty;
    public string RoomUrl { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string CustomNote { get; set; } = string.Empty;
    public Guid SentByUserId { get; set; }
    public string SentByUserName { get; set; } = string.Empty;
    public string Status { get; set; } = "SUCCESS";
    public string Channel { get; set; } = "ZALO_ZNS_SIMULATOR";
    public string MessageId { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}

public class ZaloSendBatchResponse
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;
    public int TotalSent { get; set; }
    public int SuccessCount { get; set; }
    public int FailCount { get; set; }
    public List<ZaloNotificationLogResponse> Results { get; set; } = new();
}

public class CustomerRecipientItem
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string PositionTitle { get; set; } = string.Empty;
    public int TotalNotificationsReceived { get; set; }
    public DateTime? LastNotifiedAt { get; set; }
}
