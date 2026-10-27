using ConsoleBot.Core.DataAccess;
using ConsoleBot.Core.Entities;

namespace ConsoleBot.Infrastructure.DataAccess;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<ToDoUser> userList = [];

    public ToDoUser? GetUser(Guid userId)
    {
        return userList.SingleOrDefault(user => user.UserId.Equals(userId));
    }

    public ToDoUser? GetUserByTelegramUserId(long telegramUserId)
    {
        return userList.SingleOrDefault(user => user.TelegramUserId == telegramUserId);
    }

    public void Add(ToDoUser user)
    {
        userList.Add(user);
    }
}
