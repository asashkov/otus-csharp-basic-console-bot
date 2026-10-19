namespace ConsoleBot;

internal class UserService : IUserService
{
    private ToDoUser? _toDoUser;

    public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
    {
        _toDoUser = new ToDoUser(telegramUserId, telegramUserName);

        return _toDoUser;
    }

    public ToDoUser? GetUser(long telegramUserId)
    {
        if (_toDoUser is not null && _toDoUser.TelegramUserId == telegramUserId)
            return _toDoUser;

        return null;
    }
}
