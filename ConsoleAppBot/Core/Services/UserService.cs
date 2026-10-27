using ConsoleBot.Core.DataAccess;
using ConsoleBot.Core.Entities;

namespace ConsoleBot.Core.Services;

internal class UserService(IUserRepository userRepository) : IUserService
{
    public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
    {
        var newUser = new ToDoUser(telegramUserId, telegramUserName);

        userRepository.Add(newUser);

        return newUser;
    }

    public ToDoUser? GetUser(long telegramUserId)
    {
        return userRepository.GetUserByTelegramUserId(telegramUserId);
    }
}
