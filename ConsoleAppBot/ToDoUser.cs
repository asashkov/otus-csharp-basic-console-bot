namespace ConsoleBot;

public class ToDoUser
{
    public Guid UserId { get; init; }

    public string TelegramUserName { get; set; }

    public long TelegramUserId { get; set; }

    public DateTime RegisteredAt { get; private set; }

    public ToDoUser(long telegramUserId, string telegramUserName)
    {
        TelegramUserId = telegramUserId;
        TelegramUserName = telegramUserName;
        UserId = Guid.NewGuid();
        RegisteredAt = DateTime.UtcNow;
    }
}
