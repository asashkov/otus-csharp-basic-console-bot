using ConsoleBot.Core.DataAccess;
using ConsoleBot.Core.Entities;

namespace ConsoleBot.Infrastructure.DataAccess;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<ToDoUser> userList = [];

    public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken ct)
    {
        return userList.SingleOrDefault(user => user.UserId.Equals(userId));
    }

    public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken ct)
    {
        return userList.SingleOrDefault(user => user.TelegramUserId == telegramUserId);
    }

    public async Task Add(ToDoUser user, CancellationToken ct)
    {
        userList.Add(user);
    }
}
