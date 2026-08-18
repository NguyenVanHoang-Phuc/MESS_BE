using MESS.Domain.Shared;

namespace MESS.Domain.Errors;

public static class DomainErrors
{
    public static class User
    {
        public static readonly Error NotFound = new("User.NotFound", "User was not found.");
        public static Error NotFoundById(Guid id) => new("User.NotFound", $"User with ID '{id}' was not found.");
        public static readonly Error UsernameAlreadyExists = new("User.UsernameAlreadyExists", "Username has already been taken.");
        public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "Username or password is incorrect.");
        public static readonly Error Inactive = new("User.Inactive", "User account is inactive.");
        public static readonly Error AccessDenied = new("User.AccessDenied", "You do not have permission to perform this action.");
    }

    public static class Conversation
    {
        public static readonly Error NotFound = new("Conversation.NotFound", "Conversation was not found.");
        public static Error NotFoundById(Guid id) => new("Conversation.NotFound", $"Conversation with ID '{id}' was not found.");
        public static readonly Error AccessDenied = new("Conversation.AccessDenied", "You are not a participant of this conversation.");
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
}
