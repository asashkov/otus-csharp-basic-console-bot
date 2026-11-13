using ConsoleBot.Core.DataAccess;
using ConsoleBot.Core.Entities;

namespace ConsoleBot.Core.Services;

internal class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<ToDoUser> RegisterUser(long telegramUserId, string telegramUserName, CancellationToken ct)
    {
        var newUser = new ToDoUser(telegramUserId, telegramUserName);

        await userRepository.Add(newUser, ct);

        return newUser;
    }

    public async Task<ToDoUser?> GetUser(long telegramUserId, CancellationToken ct)
    {
        return await userRepository.GetUserByTelegramUserId(telegramUserId, ct);
    }
}
