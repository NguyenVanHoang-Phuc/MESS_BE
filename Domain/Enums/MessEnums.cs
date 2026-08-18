namespace MESS.Domain.Enums;

public enum ConversationType
{
    Direct = 0,
    Group = 1
}

public enum ParticipantRole
{
    Member = 0,
    Admin = 1
}

public enum TaskStatus
{
    Todo = 0,
    InProgress = 1,
    Done = 2
}

public enum MessageType
{
    Text = 0,
    Image = 1,
    File = 2,
    Task = 3
}
