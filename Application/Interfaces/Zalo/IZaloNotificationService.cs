using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MESS.Application.DTOs.Zalo;

namespace MESS.Application.Interfaces.Zalo;

public interface IZaloNotificationService
{
    Task<ZaloSendBatchResponse> SendScheduleNotificationBatchAsync(SendZaloScheduleRequest request, Guid senderUserId, string senderName);
    Task<List<ZaloNotificationLogResponse>> GetHistoryAsync(int limit = 50);
    Task<List<ZaloNotificationLogResponse>> GetCustomerInboxAsync(Guid customerId);
    Task<List<CustomerRecipientItem>> GetRecipientsAsync();
}
