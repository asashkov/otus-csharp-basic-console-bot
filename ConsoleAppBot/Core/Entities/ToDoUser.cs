namespace ConsoleBot.Core.Entities;

public class ToDoUser
{
    public Guid UserId { get; init; }

    public string TelegramUserName { get; private set; }

    public long TelegramUserId { get; private set; }

    public DateTime RegisteredAt { get; private set; }

    public ToDoUser(long telegramUserId, string telegramUserName)
    {
        TelegramUserId = telegramUserId;
        TelegramUserName = telegramUserName;
        UserId = Guid.NewGuid();
        RegisteredAt = DateTime.UtcNow;
    }
}
