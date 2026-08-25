using System;
using MESS.Domain.Shared;

namespace MESS.Domain.Entities;

public class ZaloNotificationLog : AuditableEntity
{
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
    public string Status { get; set; } = "SUCCESS"; // SUCCESS, FAILED
    public string Channel { get; set; } = "ZALO_ZNS_SIMULATOR"; // ZALO_ZNS_SIMULATOR, ZALO_OA_BROADCAST
    public string MessageId { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
