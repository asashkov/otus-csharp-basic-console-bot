namespace ConsoleBot;

internal class ToDoUser
{
    public Guid UserId { get; init; }

    public string TelegramUserName { get; set; }

    public DateTime RegisteredAt { get; private set; }

    public ToDoUser(string telegramUserName)
    {
        TelegramUserName = telegramUserName;
        UserId = Guid.NewGuid();
        RegisteredAt = DateTime.UtcNow;
    }
}
