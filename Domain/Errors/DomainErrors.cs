using MESS.Domain.Shared;

namespace MESS.Domain.Errors;

public static class DomainErrors
{
    public static class User
    {
        public static readonly Error NotFound = new("User.NotFound", "User was not found.");
        public static Error NotFoundById(Guid id) => new("User.NotFound", $"User with ID '{id}' was not found.");
        public static readonly Error UsernameAlreadyExists = new("User.UsernameAlreadyExists", "Username has already been taken.");
        public static readonly Error EmailAlreadyExists = new("User.EmailAlreadyExists", "Email này đã được sử dụng bởi một tài khoản khác.");
        public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "Username or password is incorrect.");
        public static readonly Error Inactive = new("User.Inactive", "User account is inactive.");
        public static readonly Error AccessDenied = new("User.AccessDenied", "You do not have permission to perform this action.");
    }

    public static class Auth
    {
        public static readonly Error InvalidOtp = new("Auth.InvalidOtp", "Mã xác thực OTP không chính xác.");
        public static readonly Error OtpExpired = new("Auth.OtpExpired", "Mã xác thực OTP đã hết hạn. Vui lòng lấy mã mới.");
        public static readonly Error OtpNotFound = new("Auth.OtpNotFound", "Không tìm thấy yêu cầu xác thực OTP. Vui lòng gửi lại mã.");
    }

    public static class Conversation
    {
        public static readonly Error NotFound = new("Conversation.NotFound", "Conversation was not found.");
        public static Error NotFoundById(Guid id) => new("Conversation.NotFound", $"Conversation with ID '{id}' was not found.");
        public static readonly Error AccessDenied = new("Conversation.AccessDenied", "You are not a participant of this conversation.");
        public static readonly Error NotAdmin = new("Conversation.NotAdmin", "Only the group administrator can manage members.");
        public static readonly Error NotGroup = new("Conversation.NotGroup", "Members can only be added to or removed from a group conversation.");
        public static readonly Error ParticipantAlreadyExists = new("Conversation.ParticipantAlreadyExists", "The user is already a member of this conversation.");
        public static readonly Error ParticipantNotFound = new("Conversation.ParticipantNotFound", "The member was not found in this conversation.");
        public static readonly Error CannotRemoveAdmin = new("Conversation.CannotRemoveAdmin", "The group creator/admin cannot be removed.");
        public static readonly Error CannotCreateDirectWithSelf = new("Conversation.CannotCreateDirectWithSelf", "Cannot create a direct conversation with yourself.");
        public static readonly Error DirectConversationAlreadyExists = new("Conversation.DirectConversationAlreadyExists", "A direct conversation with this user already exists.");
    }

    public static class Message
    {
        public static readonly Error NotFound = new("Message.NotFound", "Message was not found.");
        public static Error NotFoundById(Guid id) => new("Message.NotFound", $"Message with ID '{id}' was not found.");
        public static readonly Error AccessDenied = new("Message.AccessDenied", "You are not the sender of this message.");
        public static readonly Error AlreadyRecalled = new("Message.AlreadyRecalled", "Message has already been recalled.");
        public static readonly Error AlreadyDeleted = new("Message.AlreadyDeleted", "Message has already been deleted.");
        public static readonly Error Empty = new("Message.Empty", "Nội dung tin nhắn hoặc tệp đính kèm không được để trống.");
        public static readonly Error RecallTimeExpired = new("Message.RecallTimeExpired", "Đã hết thời gian cho phép thu hồi tin nhắn (chỉ được thu hồi trong vòng 24 giờ kể từ khi gửi).");
    }

    public static class Task
    {
        public static readonly Error NotFound = new("Task.NotFound", "Task was not found.");
        public static Error NotFoundById(Guid id) => new("Task.NotFound", $"Task with ID '{id}' was not found.");
        public static readonly Error AccessDenied = new("Task.AccessDenied", "You do not have permission to modify this task.");
        public static readonly Error InvalidStatus = new("Task.InvalidStatus", "Invalid task status provided.");
    }

    public static class Department
    {
        public static readonly Error NotFound = new("Department.NotFound", "Department was not found.");
        public static readonly Error NameAlreadyExists = new("Department.NameAlreadyExists", "A department with this name already exists.");
    }

    public static class File
    {
        public static readonly Error Empty = new("File.Empty", "File cannot be empty.");
        public static readonly Error TooManyFiles = new("File.TooManyFiles", "You can only upload up to 30 files at a time.");
        public static readonly Error TooLarge = new("File.TooLarge", "File size exceeds the maximum limit of 25MB.");
        public static readonly Error InvalidFormat = new("File.InvalidFormat", "File format is not allowed.");
    }
}
